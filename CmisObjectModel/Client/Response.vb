'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
'* Contact: opensource<at>patorg.de
'* 
'* CmisObjectModelLibrary is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of CmisObjectModelLibrary.
'* 
'* This library is free software; you can redistribute it and/or
'* modify it under the terms of the GNU Lesser General Public
'* License as published by the Free Software Foundation; either
'* version 3.0 of the License, or (at your option) any later version.
'*
'* This library is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'* Lesser General Public License for more details.
'*
'* You should have received a copy of the GNU Lesser General Public
'* License along with this library (lgpl.txt).
'* If not, see <http://www.gnu.org/licenses/lgpl.txt>.
'***********************************************************************************************************************
Imports CmisObjectModel.Common
Imports CmisObjectModel.Constants
Imports ca = CmisObjectModel.AtomPub
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports ssc = System.Security.Cryptography
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Client
   ''' <summary>
   ''' Represents the response of a webservice-request
   ''' </summary>
   ''' <remarks></remarks>
   Public Class Response
      Implements IDisposable

      ''' <summary>
      ''' StatusCodes of successful requests
      ''' </summary>
      ''' <remarks></remarks>
      Protected Shared _successCodes As New System.Collections.Generic.HashSet(Of sn.HttpStatusCode) From {
         sn.HttpStatusCode.Created,
         sn.HttpStatusCode.NonAuthoritativeInformation,
         sn.HttpStatusCode.OK,
         sn.HttpStatusCode.PartialContent}

#Region "Constructors"
      Public Sub New(statusCode As sn.HttpStatusCode, message As String, contentType As String,
                     Optional contentLength As xs_Integer? = Nothing, Optional stream As IO.Stream = Nothing)
         _statusCode = statusCode
         _message = message
         _contentType = contentType
         _contentLength = contentLength
         _stream = stream
      End Sub

      Public Sub New(response As sn.HttpWebResponse)
         Dim contentTransferEncoding = response.Headers(Common.RFC2231Helper.ContentTransferEncoding)
         Dim isBase64 As Boolean = contentTransferEncoding IsNot Nothing AndAlso String.Compare(contentTransferEncoding, "base64", True) = 0
         Const _64k As Integer = &H10000

         _webResponse = response
         _statusCode = response.StatusCode
         _message = response.StatusDescription
         _contentType = response.ContentType
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
         _contentLength = If(response.ContentLength = -1, Nothing, CInt(response.ContentLength))
#Else
         _contentLength = If(response.ContentLength = -1, Nothing, response.ContentLength)
#End If
         If _successCodes.Contains(_statusCode) Then
            If isBase64 Then
               _stream = New IO.BufferedStream(New ssc.CryptoStream(response.GetResponseStream(), New ssc.FromBase64Transform(), Security.Cryptography.CryptoStreamMode.Read), _64k)
            Else
               _stream = New IO.BufferedStream(response.GetResponseStream(), _64k)
            End If
         Else
            Try
               _webResponse = Nothing
               response.Close()
            Catch
            End Try
         End If
      End Sub

      Public Sub New(exception As ss.FaultException)
         _statusCode = Common.GenericRuntimeHelper.GetStatusCode(exception)
         _message = exception.Message
         _exception = exception
         If TypeOf exception Is ssw.WebFaultException(Of Messaging.cmisFaultType) Then
            _cmisFault = CType(exception, ssw.WebFaultException(Of Messaging.cmisFaultType)).Detail
         End If
      End Sub

      Public Sub New(exception As sn.WebException)
         Dim response As sn.WebResponse = exception.Response
         Dim httpResponse As sn.HttpWebResponse = TryCast(response, sn.HttpWebResponse)

         If httpResponse Is Nothing Then
            _statusCode = Net.HttpStatusCode.InternalServerError
            _message = exception.Status.ToString()
            _exception = New ssw.WebFaultException(Of String)(_message, Net.HttpStatusCode.InternalServerError)
         Else
            _statusCode = httpResponse.StatusCode
            _message = httpResponse.StatusDescription
            _contentType = httpResponse.ContentType
            If String.IsNullOrEmpty(_contentType) Then
               _exception = New ssw.WebFaultException(Of String)(_message, _statusCode)
            Else
               Dim responseStream As IO.Stream = response.GetResponseStream()

               If _contentType.StartsWith("text/", StringComparison.InvariantCultureIgnoreCase) Then
                  Using sr As New IO.StreamReader(responseStream)
                     _errorContent = sr.ReadToEnd
                     sr.Close()
                  End Using
                  _exception = New ssw.WebFaultException(Of String)(_errorContent, httpResponse.StatusCode)
               ElseIf _contentType.StartsWith(MediaTypes.XmlApplication, StringComparison.InvariantCultureIgnoreCase) Then
                  Using sr As New IO.StreamReader(responseStream, New System.Text.UTF8Encoding(False))
                     _errorContent = sr.ReadToEnd
                     sr.Close()
                  End Using
                  Using sr As New IO.StringReader(_errorContent)
                     Dim reader As sx.XmlReader = sx.XmlReader.Create(sr)
                     Dim xmlRoot As sxs.XmlRootAttribute = GetType(Messaging.cmisFaultType).GetXmlRootAttribute(exactNonNullResult:=True)

                     reader.MoveToContent()
                     If String.Compare(If(reader.Name, ""), If(xmlRoot.ElementName, ""), True) = 0 AndAlso
                        String.Compare(If(reader.NamespaceURI, ""), If(xmlRoot.Namespace, ""), True) = 0 Then
                        _cmisFault = New Messaging.cmisFaultType
                        _cmisFault.ReadXml(reader)
                        _exception = New ssw.WebFaultException(Of Messaging.cmisFaultType)(_cmisFault, httpResponse.StatusCode)
                     Else
                        _exception = New ssw.WebFaultException(Of String)(_errorContent, httpResponse.StatusCode)
                     End If
                     reader.Close()
                     sr.Close()
                  End Using
               ElseIf _contentType.StartsWith(MediaTypes.Json, StringComparison.InvariantCultureIgnoreCase) Then
                  Using sr As New IO.StreamReader(responseStream, New System.Text.UTF8Encoding(False))
                     _errorContent = sr.ReadToEnd
                     sr.Close()
                  End Using
                  Dim serializer As New JSON.Serialization.JavaScriptSerializer()
                  _cmisFault = serializer.Deserialize(Of Messaging.cmisFaultType)(_errorContent)
                  _exception = New ssw.WebFaultException(Of Messaging.cmisFaultType)(_cmisFault, httpResponse.StatusCode)
               Else
                  _exception = New ssw.WebFaultException(Of String)(_message, _statusCode)
               End If
               responseStream.Close()
            End If
         End If
      End Sub
#End Region

#Region "IDisposable"
      Public Sub Dispose() Implements IDisposable.Dispose
         Dispose(True)
      End Sub
      Protected Overridable Sub Dispose(disposing As Boolean)
         If Not _disposed Then
            If disposing Then
               If _stream IsNot Nothing Then
                  Try
                     _stream.Close()
                     _stream = Nothing
                  Catch
                  End Try
               End If
               Try
                  _webResponse.Close()
               Catch
               End Try
            End If
            _disposed = True
         End If
      End Sub

      Protected _disposed As Boolean = False
      Public ReadOnly Property Disposed As Boolean
         Get
            Return _disposed
         End Get
      End Property
#End Region

      Protected _cmisFault As Messaging.cmisFaultType
      Public ReadOnly Property CmisFault As Messaging.cmisFaultType
         Get
            Return _cmisFault
         End Get
      End Property

      Protected _contentLength As xs_Integer?
      Public ReadOnly Property ContentLength As xs_Integer?
         Get
            Return _contentLength
         End Get
      End Property

      Protected _contentType As String
      Public ReadOnly Property ContentType As String
         Get
            Return _contentType
         End Get
      End Property

      Protected _errorContent As String
      Public ReadOnly Property ErrorContent As String
         Get
            Return _errorContent
         End Get
      End Property

      Protected _exception As ss.FaultException
      Public ReadOnly Property Exception As ss.FaultException
         Get
            Return _exception
         End Get
      End Property

      Protected _message As String
      Public ReadOnly Property Message As String
         Get
            Return _message
         End Get
      End Property

      Protected _statusCode As sn.HttpStatusCode
      Public ReadOnly Property StatusCode As sn.HttpStatusCode
         Get
            Return _statusCode
         End Get
      End Property

      Protected _stream As IO.Stream
      Public ReadOnly Property Stream As IO.Stream
         Get
            Return _stream
         End Get
      End Property

      Protected _webResponse As sn.WebResponse
      Public ReadOnly Property WebResponse As sn.WebResponse
         Get
            Return _webResponse
         End Get
      End Property

      Public Shared Widening Operator CType(value As Response) As IO.Stream
         Return If(value Is Nothing, Nothing, value._stream)
      End Operator
      Public Shared Widening Operator CType(value As ss.FaultException) As Response
         Return New Response(value)
      End Operator
      Public Shared Widening Operator CType(value As IO.Stream) As Response
         Return New Response(Net.HttpStatusCode.OK, "OK", MediaTypes.Stream, Stream:=value)
      End Operator
   End Class
End Namespace