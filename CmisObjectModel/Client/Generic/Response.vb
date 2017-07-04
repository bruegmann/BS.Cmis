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

Namespace CmisObjectModel.Client.Generic
   ''' <summary>
   ''' Response differs from void or stream
   ''' </summary>
   ''' <typeparam name="TResponse"></typeparam>
   ''' <remarks></remarks>
   Public Class Response(Of TResponse)
      Inherits Response

      Public Sub New(response As TResponse, contentType As String)
         MyBase.New(Net.HttpStatusCode.OK, "OK", contentType)
         _response = response
      End Sub
      Public Sub New(response As TResponse, contentType As String, exception As ss.FaultException)
         MyBase.New(exception)
         _response = response
         _contentType = contentType
      End Sub
      Public Sub New(statusCode As sn.HttpStatusCode, message As String, contentType As String, response As TResponse)
         MyBase.New(statusCode, message, contentType)
         _response = response
      End Sub
      Public Sub New(response As sn.HttpWebResponse, responseFactory As Func(Of sx.XmlReader, TResponse))
         MyBase.New(response)

         'create TResponse from stream
         CreateResponseFromStream(Sub(ms)
                                     'evaluate response-stream
                                     Using sr As New IO.StreamReader(ms, New System.Text.UTF8Encoding(False))
                                        Dim reader As sx.XmlReader = sx.XmlReader.Create(sr)
                                        _response = responseFactory.Invoke(reader)
                                        reader.Close()
                                     End Using
                                  End Sub)
      End Sub
      Public Sub New(response As sn.HttpWebResponse, responseFactory As Func(Of IO.MemoryStream, String, TResponse))
         MyBase.New(response)

         'create TResponse from stream
         CreateResponseFromStream(Sub(ms)
                                     'evaluate response-stream
                                     _response = responseFactory.Invoke(ms, _contentType)
                                  End Sub)
      End Sub
      Public Sub New(response As sn.HttpWebResponse, responseFactory As Func(Of String, String, TResponse))
         MyBase.New(response)

         'create TResponse from stream
         CreateResponseFromStream(Sub(ms)
                                     'evaluate response-stream
                                     _response = responseFactory.Invoke(System.Text.Encoding.UTF8.GetString(ms.ToArray()), _contentType)
                                  End Sub)
      End Sub
      Private Sub CreateResponseFromStream(responseFactory As Action(Of System.IO.MemoryStream))
         If _stream IsNot Nothing Then
            Using ms As New IO.MemoryStream
               _stream.CopyTo(ms)
               Try
                  _stream.Close()
               Catch
               End Try
               _stream = New IO.MemoryStream(ms.ToArray())

               Try
                  If ms.Length <> 0 Then
                     ms.Position = 0
                     responseFactory.Invoke(ms)
                  End If
               Catch ex As ssw.WebFaultException
                  _statusCode = ex.StatusCode
                  _message = ex.Message
                  _exception = ex
               Catch ex As ssw.WebFaultException(Of String)
                  _statusCode = ex.StatusCode
                  _message = ex.Detail
                  _exception = ex
               Catch ex As ssw.WebFaultException(Of Messaging.cmisFaultType)
                  _statusCode = ex.StatusCode
                  _message = ex.Detail.Message
                  _exception = ex
               Catch ex As ssw.WebFaultException(Of Exception)
                  _statusCode = ex.StatusCode
                  _message = ex.Message
                  _exception = ex
               Catch ex As ss.FaultException
                  _statusCode = sn.HttpStatusCode.InternalServerError
                  _message = ex.Message
                  _exception = ex
               Catch ex As sx.XmlException
                  _statusCode = sn.HttpStatusCode.ExpectationFailed
                  _message = ex.Message
                  _exception = New ssw.WebFaultException(Of Exception)(ex, sn.HttpStatusCode.ExpectationFailed)
               Catch ex As Exception
                  _statusCode = sn.HttpStatusCode.InternalServerError
                  _message = ex.Message
               End Try
               ms.Close()
            End Using
         End If
      End Sub
      Public Sub New(exception As ss.FaultException)
         MyBase.New(exception)
      End Sub
      Public Sub New(exception As sn.WebException)
         MyBase.New(exception)
      End Sub

      Private _response As TResponse
      Public ReadOnly Property Response As TResponse
         Get
            Return _response
         End Get
      End Property

      Public Shared Shadows Widening Operator CType(value As Response(Of TResponse)) As TResponse
         Return If(value Is Nothing, Nothing, value.Response)
      End Operator
      Public Shared Shadows Widening Operator CType(value As TResponse) As Response(Of TResponse)
         Dim responseType As Type = GetType(TResponse)

         If GetType(ca.AtomEntry).IsAssignableFrom(responseType) Then
            Return New Response(Of TResponse)(value, MediaTypes.Entry)
         ElseIf GetType(ca.AtomFeed).IsAssignableFrom(responseType) Then
            Return New Response(Of TResponse)(value, MediaTypes.Feed)
         ElseIf GetType(ca.AtomCollectionInfo).IsAssignableFrom(responseType) Then
            Return New Response(Of TResponse)(value, MediaTypes.Xml)
         ElseIf GetType(ca.AtomLink).IsAssignableFrom(responseType) Then
            Return New Response(Of TResponse)(value, MediaTypes.Xml)
         ElseIf GetType(ca.AtomWorkspace).IsAssignableFrom(responseType) Then
            Return New Response(Of TResponse)(value, MediaTypes.Xml)
         ElseIf GetType(String) Is responseType Then
            Return New Response(Of TResponse)(value, MediaTypes.PlainText)
         ElseIf GetType(Core.Security.cmisAccessControlListType).IsAssignableFrom(responseType) Then
            Return New Response(Of TResponse)(value, MediaTypes.Acl)
         ElseIf GetType(Core.cmisAllowableActionsType).IsAssignableFrom(responseType) Then
            Return New Response(Of TResponse)(value, MediaTypes.AllowableActions)
         ElseIf GetType(ca.AtomServiceDocument).IsAssignableFrom(responseType) Then
            Return New Response(Of TResponse)(value, MediaTypes.Service)
         ElseIf GetType(Serialization.XmlSerializable).IsAssignableFrom(GetType(TResponse)) Then
            Return New Response(Of TResponse)(value, MediaTypes.Xml)
         ElseIf GetType(sxs.IXmlSerializable).IsAssignableFrom(GetType(TResponse)) Then
            Return New Response(Of TResponse)(value, MediaTypes.XmlApplication)
         Else
            Return New Response(Of TResponse)(value, Nothing)
         End If
      End Operator
      Public Shared Shadows Widening Operator CType(value As ss.FaultException) As Response(Of TResponse)
         Return New Response(Of TResponse)(value)
      End Operator

   End Class
End Namespace