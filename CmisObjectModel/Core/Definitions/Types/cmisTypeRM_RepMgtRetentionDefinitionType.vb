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

Namespace CmisObjectModel.Core.Definitions.Types
   <Attributes.CmisTypeInfo(cmisTypeRM_RepMgtRetentionDefinitionType.CMISTypeName, cmisTypeRM_RepMgtRetentionDefinitionType.TargetTypeName, cmisTypeRM_RepMgtRetentionDefinitionType.DefaultElementName)>
   Partial Public Class cmisTypeRM_RepMgtRetentionDefinitionType

      Public Sub New(id As String, localName As String, displayName As String, queryName As String,
                     ParamArray propertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType())
         MyBase.New(id, localName, displayName, queryName, cmisTypeSecondaryDefinitionType.TargetTypeName, propertyDefinitions)
      End Sub
      Public Sub New(id As String, localName As String, displayName As String, queryName As String,
                     parentId As String,
                     ParamArray propertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType())
         MyBase.New(id, localName, displayName, queryName, parentId, propertyDefinitions)
      End Sub

#Region "Constants"
      Public Shadows Const CMISTypeName As String = "cmis:cmisTypeRM_RepMgtRetentionDefinitionType"
      Public Shadows Const TargetTypeName As String = "cmis:rm_repMgtRetention"
      Public Shadows Const DefaultElementName As String = "typeRM_RepMgtRetentionDefinition"
#End Region

      ''' <summary>
      ''' Returns the defaultProperties of a TypeRM_HoldDefinition-instance
      ''' </summary>
      Public Shared Shadows Function GetDefaultProperties(localNamespace As String, isBaseType As Boolean) As List(Of Core.Definitions.Properties.cmisPropertyDefinitionType)
         Return cmisTypeSecondaryDefinitionType.GetDefaultProperties(localNamespace, False)
      End Function

      Protected Overrides Function GetCmisTypeName() As String
         Return CMISTypeName
      End Function

      Protected Overrides Sub InitClass()
         MyBase.InitClass()
         _parentId = cmisTypeSecondaryDefinitionType.TargetTypeName
         _queryable = True
      End Sub

   End Class
End Namespace