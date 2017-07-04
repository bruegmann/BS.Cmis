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
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Serialization
   ''' <summary>
   ''' A simple instantiatable XmlSerializable
   ''' </summary>
   ''' <remarks></remarks>
   Public NotInheritable Class EmptyXmlSerializable
      Inherits XmlSerializable

#Region "INotifyPropertyChanged"
      Public Overrides Sub [AddHandler](handler As System.ComponentModel.PropertyChangedEventHandler, propertyName As String)
      End Sub
      Public Overrides Sub [AddHandler](handler As System.ComponentModel.PropertyChangedEventHandler, ParamArray propertyNames() As String)
      End Sub
      Public Overrides Sub [RemoveHandler](handler As System.ComponentModel.PropertyChangedEventHandler, propertyName As String)
      End Sub
      Public Overrides Sub [RemoveHandler](handler As System.ComponentModel.PropertyChangedEventHandler, ParamArray propertyNames() As String)
      End Sub
#End Region

#Region "IXmlSerializable"
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As XmlAttributeOverrides)
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As XmlAttributeOverrides)
      End Sub
#End Region

      Public Shared ReadOnly Singleton As New EmptyXmlSerializable()
   End Class
End Namespace