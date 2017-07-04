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
Imports ssw = System.ServiceModel.Web
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Messaging
   <sxs.XmlRoot(ElementName:="cmisFault", Namespace:=Namespaces.cmism)>
   Partial Public Class cmisFaultType

#Region "Constructors"
      Public Sub New(type As Messaging.enumServiceException, message As String)
         Me.New(type.ToHttpStatusCode(), type, message)
      End Sub

      Public Sub New(code As System.Net.HttpStatusCode, type As Messaging.enumServiceException, message As String)
         _code = code
         _message = message
         _type = type
      End Sub

      ''' <summary>
      ''' Creates a cmisFaultType-instance from ex
      ''' </summary>
      ''' <param name="ex"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(ex As Exception) As cmisFaultType
         Dim extensions As Extensions.Extension() = New Extensions.Extension() {New ExceptionExtension(ex)}
         Return New cmisFaultType(Net.HttpStatusCode.InternalServerError, enumServiceException.runtime, ex.Message) With {
            ._extensions = extensions}
      End Function

      Public Shared Function CreateInstance(ex As ssw.WebFaultException) As cmisFaultType
         Dim extensions As Extensions.Extension() = New Extensions.Extension() {New ExceptionExtension(ex)}
         Return New cmisFaultType(ex.StatusCode, ex.StatusCode.ToServiceException, ex.Message) With {._extensions = extensions}
      End Function

      Public Shared Function CreateInstance(Of T)(ex As ssw.WebFaultException(Of T)) As cmisFaultType
         Dim extensions As Extensions.Extension() = New Extensions.Extension() {New ExceptionExtension(ex)}
         If GetType(T) Is GetType(String) Then
            Return New cmisFaultType(ex.StatusCode, ex.StatusCode.ToServiceException, ex.Message & vbCrLf & CType(CObj(ex), ssw.WebFaultException(Of String)).Detail) With {._extensions = extensions}
         ElseIf GetType(T) Is GetType(cmisFaultType) Then
            Dim retVal As cmisFaultType = CType(CObj(ex), ssw.WebFaultException(Of cmisFaultType)).Detail
            If retVal._extensions Is Nothing Then
               retVal._extensions = extensions
            Else
               With New System.Collections.Generic.List(Of Extensions.Extension)(retVal._extensions.Length + extensions.Length)
                  .AddRange(retVal._extensions)
                  .AddRange(extensions)
                  retVal._extensions = .ToArray()
               End With
            End If
            Return retVal
         Else
            Return New cmisFaultType(ex.StatusCode, ex.StatusCode.ToServiceException, ex.Message) With {._extensions = extensions}
         End If
      End Function
#End Region

#Region "Helper-classes"
      ''' <summary>
      ''' Extension to encapsulate non WebExceptions
      ''' </summary>
      ''' <remarks></remarks>
      <sxs.XmlRoot(ElementName:="exceptionExtension", Namespace:=Namespaces.cmism),
       Attributes.CmisTypeInfo("cmism:exceptionExtension", Nothing, "exceptionExtension")>
      Public Class ExceptionExtension
         Inherits Extensions.Extension

#Region "Constructors"
         Public Sub New()
         End Sub

         Public Sub New(ex As Exception)
            _exceptionTypeName = ex.GetType().FullName
            _source = ex.Source
            _stackTrace = ex.StackTrace
            If ex.InnerException IsNot Nothing Then _innerException = cmisFaultType.CreateInstance(ex.InnerException)
         End Sub
#End Region

#Region "IXmlSerializable"
         Private Shared _setter As New Dictionary(Of String, Action(Of ExceptionExtension, String)) From {
            {"exceptiontypename", AddressOf SetExceptionTypeName}} '_setter

         Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
            'at least one property is serialized in an attribute-value
            If _setter.Count > 0 Then
               For attributeIndex As Integer = 0 To reader.AttributeCount - 1
                  reader.MoveToAttribute(attributeIndex)
                  'attribute name
                  Dim key As String = reader.Name.ToLowerInvariant
                  If _setter.ContainsKey(key) Then _setter(key).Invoke(Me, reader.GetAttribute(attributeIndex))
               Next
            End If
         End Sub

         Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
            _innerException = Read(Of cmisFaultType)(reader, attributeOverrides, "innerException", Namespaces.cmism, AddressOf GenericXmlSerializableFactory(Of cmisFaultType))
            _source = Read(reader, attributeOverrides, "source", Namespaces.cmism, _source)
            _stackTrace = Read(reader, attributeOverrides, "stackTrace", Namespaces.cmism, _stackTrace)
         End Sub

         Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
            WriteAttribute(writer, attributeOverrides, "exceptionTypeName", Nothing, _exceptionTypeName)
            WriteElement(writer, attributeOverrides, "innerException", Namespaces.cmism, _innerException)
            If Not String.IsNullOrEmpty(_source) Then WriteElement(writer, attributeOverrides, "source", Namespaces.cmism, _source)
            If Not String.IsNullOrEmpty(_stackTrace) Then WriteElement(writer, attributeOverrides, "stackTrace", Namespaces.cmism, _stackTrace)
         End Sub
#End Region

         Protected _exceptionTypeName As String
         Public Property ExceptionTypeName As String
            Get
               Return _exceptionTypeName
            End Get
            Set(value As String)
               If value <> _exceptionTypeName Then
                  Dim oldValue As String = _exceptionTypeName
                  _exceptionTypeName = value
                  OnPropertyChanged("ExceptionTypeName", value, oldValue)
               End If
            End Set
         End Property 'ExceptionTypeName
         Private Shared Sub SetExceptionTypeName(instance As ExceptionExtension, value As String)
            instance.ExceptionTypeName = value
         End Sub

         Protected _innerException As cmisFaultType
         Public Property InnerException As cmisFaultType
            Get
               Return _innerException
            End Get
            Set(value As cmisFaultType)
               If value IsNot _innerException Then
                  Dim oldValue As cmisFaultType = _innerException
                  _innerException = value
                  OnPropertyChanged("InnerException", value, oldValue)
               End If
            End Set
         End Property 'InnerException

         Protected _source As String
         Public Property Source As String
            Get
               Return _source
            End Get
            Set(value As String)
               If _source <> value Then
                  Dim oldValue As String = _source
                  _source = value
                  OnPropertyChanged("Source", value, oldValue)
               End If
            End Set
         End Property 'Source

         Protected _stackTrace As String
         Public Property StackTrace As String
            Get
               Return _stackTrace
            End Get
            Set(value As String)
               If value <> _stackTrace Then
                  Dim oldValue As String = _stackTrace
                  _stackTrace = value
                  OnPropertyChanged("StackTrace", value, oldValue)
               End If
            End Set
         End Property 'StackTrace

      End Class
#End Region

      ''' <summary>
      ''' Generates a WebFaultException(Of cmisFaultType) for an invalid argument exception
      ''' </summary>
      ''' <param name="argumentNameOrErrorText">To define a MUST-NOT-be-null-or-empty-InvalidArgumentException set this parameter
      ''' to the name of the invalid argument and set the isNotNullOrEmptyException-parameter to True. Otherwise this parameter
      ''' must contain the complete errorText and the isNotNullOrEmptyException must be set to False.</param>
      ''' <param name="isNotNullOrEmptyException">If True the argumentNameOrErrorText-parameter is handled as argumentName</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInvalidArgumentException(argumentNameOrErrorText As String,
                                                            Optional isNotNullOrEmptyException As Boolean = True) As ssw.WebFaultException(Of cmisFaultType)
         Dim httpStatusCode As System.Net.HttpStatusCode = Common.ToHttpStatusCode(enumServiceException.invalidArgument)

         Return New ssw.WebFaultException(Of cmisFaultType)(New cmisFaultType(httpStatusCode, enumServiceException.invalidArgument,
                                                                              If(isNotNullOrEmptyException, String.Format(My.Resources.InvalidArgument, argumentNameOrErrorText), argumentNameOrErrorText)),
                                                            httpStatusCode)
      End Function

      ''' <summary>
      ''' Generates a WebFaultException(Of cmisFaultType) for a not found exception
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateNotFoundException(Optional id As String = Nothing) As ssw.WebFaultException(Of cmisFaultType)
         Dim httpStatusCode As System.Net.HttpStatusCode = Common.ToHttpStatusCode(enumServiceException.objectNotFound)

         Return New ssw.WebFaultException(Of cmisFaultType)(New cmisFaultType(httpStatusCode, enumServiceException.objectNotFound, "Object " & If(id Is Nothing, "", "'" & id & "' ") & "not found."), httpStatusCode)
      End Function

      Public Shared Function CreateNotSupportedException(methodName As String) As ssw.WebFaultException(Of cmisFaultType)
         Dim httpStatusCode As System.Net.HttpStatusCode = Common.ToHttpStatusCode(enumServiceException.notSupported)

         Return New ssw.WebFaultException(Of cmisFaultType)(New cmisFaultType(httpStatusCode, enumServiceException.notSupported, String.Format(My.Resources.NotSupported, methodName)), httpStatusCode)
      End Function

      Public Shared Function CreatePermissionDeniedException() As ssw.WebFaultException(Of cmisFaultType)
         Dim httpStatusCode As System.Net.HttpStatusCode = Common.ToHttpStatusCode(enumServiceException.permissionDenied)

         Return New ssw.WebFaultException(Of cmisFaultType)(New cmisFaultType(httpStatusCode, enumServiceException.permissionDenied, "Permission denied."), httpStatusCode)
      End Function

      ''' <summary>
      ''' Generates a WebFaultException(Of cmisFaultType) for an unknown error
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateUnknownException() As ssw.WebFaultException(Of cmisFaultType)
         Dim httpStatusCode As System.Net.HttpStatusCode = Common.ToHttpStatusCode(enumServiceException.runtime)

         Return New ssw.WebFaultException(Of cmisFaultType)(New cmisFaultType(httpStatusCode, enumServiceException.runtime, "Unknown server error."), httpStatusCode)
      End Function

      ''' <summary>
      ''' Generates a WebFaultException(Of cmisFaultType) for an unspecific exception
      ''' </summary>
      ''' <param name="ex"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateUnknownException(ex As Exception) As ssw.WebFaultException(Of cmisFaultType)
         Return New ssw.WebFaultException(Of cmisFaultType)(CreateInstance(ex), Net.HttpStatusCode.InternalServerError)
      End Function

      ''' <summary>
      ''' Creates WebFaultException(Of cmisFaultType) containing this instance
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ToFaultException() As ssw.WebFaultException(Of cmisFaultType)
         Return New ssw.WebFaultException(Of cmisFaultType)(Me, CType(CObj(_code), System.Net.HttpStatusCode))
      End Function

   End Class
End Namespace