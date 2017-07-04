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
   <Attributes.CmisTypeInfo(cmisTypeFolderDefinitionType.CMISTypeName, cmisTypeFolderDefinitionType.TargetTypeName, cmisTypeFolderDefinitionType.DefaultElementName)>
   Partial Public Class cmisTypeFolderDefinitionType

      Public Sub New(id As String, localName As String, displayName As String, queryName As String,
                     ParamArray propertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType())
         MyBase.New(id, localName, displayName, queryName, propertyDefinitions)
      End Sub
      Public Sub New(id As String, localName As String, displayName As String, queryName As String,
                     parentId As String,
                     ParamArray propertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType())
         MyBase.New(id, localName, displayName, queryName, parentId, propertyDefinitions)
      End Sub

#Region "Constants"
      Public Shadows Const CMISTypeName As String = "cmis:cmisTypeFolderDefinitionType"
      Public Const TargetTypeName As String = "cmis:folder"
      Public Const DefaultElementName As String = "typeFolderDefinition"
#End Region

      Protected Overrides ReadOnly Property _baseId As enumBaseObjectTypeIds
         Get
            Return enumBaseObjectTypeIds.cmisFolder
         End Get
      End Property

      ''' <summary>
      ''' Returns the defaultProperties of a TypeFolderDefinition-instance
      ''' </summary>
      ''' <param name="localNamespace"></param>
      ''' <param name="isBaseType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function GetDefaultProperties(localNamespace As String, isBaseType As Boolean) As List(Of Core.Definitions.Properties.cmisPropertyDefinitionType)
         With New Common.PredefinedPropertyDefinitionFactory(localNamespace, isBaseType)
            Return New List(Of Core.Definitions.Properties.cmisPropertyDefinitionType) From {
               .Name, .Description, .ObjectId, .BaseTypeId, .ObjectTypeId, .SecondaryObjectTypeIds, .CreatedBy, .CreationDate,
               .LastModifiedBy, .LastModificationDate, .ChangeToken, .ParentId, .Path, .AllowedChildObjectTypeIds}
         End With
      End Function

      Protected Overrides Function GetCmisTypeName() As String
         Return CMISTypeName
      End Function

      Protected Overrides Sub InitClass()
         MyBaseInitClass()
         _fileable = True
         _id = TargetTypeName
         _queryable = True
         _queryName = TargetTypeName
      End Sub

   End Class
End Namespace