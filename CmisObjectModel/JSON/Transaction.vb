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
Imports cs = CmisObjectModel.Serialization
Imports sxs = System.Xml.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON
   ''' <summary>
   ''' Allow Web-Applications to get access to the last result of a POST-request
   ''' </summary>
   ''' <remarks>see http://docs.oasis-open.org/cmis/CMIS/v1.1/os/CMIS-v1.1-os.html
   ''' 5.4.4.4  Access to Form Response Content</remarks>
   <sxs.XmlRoot("transaction", Namespace:=Constants.Namespaces.browser)>
   Public Class Transaction
      Inherits cs.XmlSerializable

#Region "IXmlSerializable"
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As cs.XmlAttributeOverrides)
         _code = Read(reader, attributeOverrides, "code", Constants.Namespaces.browser, _code)
         _objectId = Read(reader, attributeOverrides, "objectId", Constants.Namespaces.browser, _objectId)
         _exception = Read(reader, attributeOverrides, "exception", Constants.Namespaces.browser, _exception)
         _message = Read(reader, attributeOverrides, "message", Constants.Namespaces.browser, _message)
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As cs.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "code", Constants.Namespaces.browser, Convert(_code))
         If Not String.IsNullOrEmpty(_objectId) Then WriteElement(writer, attributeOverrides, "objectId", Constants.Namespaces.browser, _objectId)
         If Not String.IsNullOrEmpty(_exception) Then WriteElement(writer, attributeOverrides, "exception", Constants.Namespaces.browser, _exception)
         If Not String.IsNullOrEmpty(_message) Then WriteElement(writer, attributeOverrides, "message", Constants.Namespaces.browser, _message)
      End Sub
#End Region

      Protected _code As xs_Integer
      Public Overridable Property Code As xs_Integer
         Get
            Return _code
         End Get
         Set(value As xs_Integer)
            Dim oldValue As xs_Integer = _code

            If value <> _code Then
               _code = value
               OnPropertyChanged("Code", value, oldValue)
            End If
         End Set
      End Property 'Code

      Protected _exception As String
      Public Overridable Property Exception As String
         Get
            Return _exception
         End Get
         Set(value As String)
            Dim oldValue As String = _exception

            If value <> _exception Then
               _exception = value
               OnPropertyChanged("Exception", value, oldValue)
            End If
         End Set
      End Property 'Exception

      Protected _message As String
      Public Overridable Property Message As String
         Get
            Return _message
         End Get
         Set(value As String)
            Dim oldValue As String = _message

            If value <> _message Then
               _exception = value
               OnPropertyChanged("Message", value, oldValue)
            End If
         End Set
      End Property 'Message

      Protected _objectId As String
      Public Overridable Property ObjectId As String
         Get
            Return _objectId
         End Get
         Set(value As String)
            Dim oldValue As String = _objectId

            If value <> _objectId Then
               _objectId = value
               OnPropertyChanged("ObjectId", value, oldValue)
            End If
         End Set
      End Property 'ObjectId

   End Class
End Namespace