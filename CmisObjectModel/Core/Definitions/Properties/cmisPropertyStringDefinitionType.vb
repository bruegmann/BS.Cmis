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
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Core.Definitions.Properties
   <sxs.XmlRoot(cmisPropertyStringDefinitionType.DefaultElementName, Namespace:=Constants.Namespaces.cmis),
    Attributes.CmisTypeInfo(cmisPropertyStringDefinitionType.CmisTypeName, cmisPropertyStringDefinitionType.TargetTypeName, cmisPropertyStringDefinitionType.DefaultElementName)>
   Partial Public Class cmisPropertyStringDefinitionType

      Public Sub New(id As String, localName As String, localNamespace As String, displayName As String, queryName As String,
                     required As Boolean, inherited As Boolean, queryable As Boolean, orderable As Boolean,
                     cardinality As enumCardinality, updatability As enumUpdatability,
                     ParamArray choices As Core.Choices.cmisChoiceString())
         MyBase.New(id, localName, localNamespace, displayName, queryName,
                    required, inherited, queryable, orderable, cardinality, updatability, choices)
      End Sub
      Public Sub New(id As String, localName As String, localNamespace As String, displayName As String, queryName As String,
                     required As Boolean, inherited As Boolean, queryable As Boolean, orderable As Boolean,
                     cardinality As enumCardinality, updatability As enumUpdatability, defaultValue As Core.Properties.cmisPropertyString,
                     ParamArray choices As Core.Choices.cmisChoiceString())
         MyBase.New(id, localName, localNamespace, displayName, queryName,
                    required, inherited, queryable, orderable, cardinality, updatability, defaultValue, choices)
      End Sub

#Region "Constants"
      Public Const CmisTypeName As String = "cmis:cmisPropertyStringDefinitionType"
      Public Const TargetTypeName As String = Core.Properties.cmisPropertyString.CmisTypeName
      Public Const DefaultElementName As String = "propertyStringDefinition"
#End Region

      Protected Overrides ReadOnly Property _propertyType As enumPropertyType
         Get
            Return enumPropertyType.string
         End Get
      End Property

   End Class
End Namespace