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

Namespace CmisObjectModel.Messaging
   Partial Public Class cmisContentStreamType

      Public Sub New(stream As IO.Stream, filename As String, mimeType As String,
                     Optional internalUsage As Boolean = False)
         If stream IsNot Nothing Then
            Dim ms As New IO.MemoryStream()
            stream.CopyTo(ms)
            ms.Position = 0
            BinaryStream(internalUsage) = ms
         End If
         _filename = filename
         _mimeType = mimeType
      End Sub

      Public Sub New(stream As IO.Stream, filename As String, mimeType As String,
                     result As enumGetContentStreamResult,
                     Optional internalUsage As Boolean = True)
         Me.New(stream, filename, mimeType, internalUsage)
         _result = result
      End Sub

      Private _binaryStream As IO.MemoryStream
      Public ReadOnly Property BinaryStream As IO.Stream
         Get
            If _binaryStream IsNot Nothing Then
               Return _binaryStream
            ElseIf String.IsNullOrEmpty(_stream) Then
               Return Nothing
            Else
               Return New IO.MemoryStream(System.Convert.FromBase64String(_stream))
            End If
         End Get
      End Property
      ''' <summary>
      ''' Writes the binary stream
      ''' </summary>
      ''' <param name="internalUseOnly">Set this parameter to false to update the Stream- and Length-property also</param>
      ''' <value></value>
      ''' <remarks></remarks>
      Public WriteOnly Property BinaryStream(internalUseOnly As Boolean) As IO.MemoryStream
         Set(value As IO.MemoryStream)
            Dim oldValue As IO.MemoryStream = _binaryStream

            _binaryStream = value
            If Not internalUseOnly Then
               If value Is Nothing Then
                  _stream = Nothing
                  _length = 0
                  _result = enumGetContentStreamResult.NotSet
               Else
                  Stream = System.Convert.ToBase64String(value.ToArray())
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
                  Length = CInt(value.Length)
#Else
                  Length = value.Length
#End If
                  _result = enumGetContentStreamResult.Content
               End If
            End If
            OnPropertyChanged("BinaryStream", value, oldValue)
         End Set
      End Property

      ''' <summary>
      ''' If necessary the method appends missed appropriate properties (cmis:contentStreamFileName,
      ''' cmis:contentStreamLength and cmis:contentStreamMimeType) for the instance-properties
      ''' FileName, Length and MimeType
      ''' </summary>
      ''' <param name="cmisObject"></param>
      ''' <remarks></remarks>
      Public Sub ExtendProperties(cmisObject As Core.cmisObjectType)
         If cmisObject IsNot Nothing Then cmisObject.Properties = ExtendProperties(cmisObject.Properties)
      End Sub
      ''' <summary>
      ''' If necessary the method appends missed appropriate properties (cmis:contentStreamFileName,
      ''' cmis:contentStreamLength and cmis:contentStreamMimeType) for the instance-properties
      ''' FileName, Length and MimeType
      ''' </summary>
      ''' <param name="properties"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ExtendProperties(properties As Core.Collections.cmisPropertiesType) As Core.Collections.cmisPropertiesType
         If Not String.IsNullOrEmpty(_filename) OrElse _length.HasValue OrElse Not String.IsNullOrEmpty(_mimeType) Then
            Dim factory As New PredefinedPropertyDefinitionFactory(Nothing)

            If properties Is Nothing Then properties = New Core.Collections.cmisPropertiesType
            With properties.FindProperties(True, CmisPredefinedPropertyNames.ContentStreamFileName,
                                                 CmisPredefinedPropertyNames.ContentStreamLength,
                                                 CmisPredefinedPropertyNames.ContentStreamMimeType)
               If Not (String.IsNullOrEmpty(_filename) OrElse
                       .ContainsKey(CmisPredefinedPropertyNames.ContentStreamFileName)) Then
                  properties.Append(factory.ContentStreamFileName.CreateProperty(_filename))
               End If
               If _length.HasValue AndAlso
                  Not .ContainsKey(CmisPredefinedPropertyNames.ContentStreamLength) Then
                  properties.Append(factory.ContentStreamLength.CreateProperty(_length.Value))
               End If
               If Not (String.IsNullOrEmpty(_mimeType) OrElse
                       .ContainsKey(CmisPredefinedPropertyNames.ContentStreamMimeType)) Then
                  properties.Append(factory.ContentStreamMimeType.CreateProperty(_mimeType))
               End If
            End With
         End If

         Return properties
      End Function

      Private _result As enumGetContentStreamResult = enumGetContentStreamResult.NotSet
      Public Property Result As enumGetContentStreamResult
         Get
            Return _result
         End Get
         Set(value As enumGetContentStreamResult)
            _result = value
         End Set
      End Property

      Public ReadOnly Property StatusCode As System.Net.HttpStatusCode
         Get
            Return CType(CInt(_result), System.Net.HttpStatusCode)
         End Get
      End Property

   End Class
End Namespace