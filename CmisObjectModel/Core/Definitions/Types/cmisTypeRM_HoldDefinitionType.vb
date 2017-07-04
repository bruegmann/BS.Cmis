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
   <Attributes.CmisTypeInfo(cmisTypeRM_HoldDefinitionType.CMISTypeName, cmisTypeRM_HoldDefinitionType.TargetTypeName, cmisTypeRM_HoldDefinitionType.DefaultElementName)>
   Partial Public Class cmisTypeRM_HoldDefinitionType

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
      Public Shadows Const CMISTypeName As String = "cmis:cmisTypeRM_HoldDefinitionType"
      Public Shadows Const TargetTypeName As String = "cmis:rm_hold"
      Public Shadows Const DefaultElementName As String = "typeRM_HoldDefinition"
#End Region

      ''' <summary>
      ''' Returns the defaultProperties of a TypeRM_HoldDefinition-instance
      ''' </summary>
      Public Shared Shadows Function GetDefaultProperties(localNamespace As String, isBaseType As Boolean,
                                                          Optional holdIdsQueryable As Boolean = True,
                                                          Optional holdIdsUpdatability As enumUpdatability = enumUpdatability.readwrite) As List(Of Core.Definitions.Properties.cmisPropertyDefinitionType)
         Dim retVal As List(Of Core.Definitions.Properties.cmisPropertyDefinitionType) = cmisTypeSecondaryDefinitionType.GetDefaultProperties(localNamespace, False)

         With New Common.PredefinedPropertyDefinitionFactory(localNamespace, isBaseType)
            retVal.Add(.RM_HoldIds(queryable:=holdIdsQueryable, updatability:=holdIdsUpdatability))
         End With

         Return retVal
      End Function

      Protected Overrides Function GetCmisTypeName() As String
         Return CMISTypeName
      End Function

   End Class
End Namespace