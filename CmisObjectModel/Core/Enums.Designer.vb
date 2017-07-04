'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'* Author: auto-generated code
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
Imports srs = System.Runtime.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core
   Public Enum enumACLPropagation As Integer
      repositorydetermined
      objectonly
      propagate
   End Enum

   Public Enum enumAllowableActionsKey As Integer
      <srs.EnumMember(Value:="canGetDescendents.Folder")>
      canGetDescendentsFolder
      <srs.EnumMember(Value:="canGetChildren.Folder")>
      canGetChildrenFolder
      <srs.EnumMember(Value:="canGetParents.Folder")>
      canGetParentsFolder
      <srs.EnumMember(Value:="canGetFolderParent.Object")>
      canGetFolderParentObject
      <srs.EnumMember(Value:="canCreateDocument.Folder")>
      canCreateDocumentFolder
      <srs.EnumMember(Value:="canCreateFolder.Folder")>
      canCreateFolderFolder
      <srs.EnumMember(Value:="canCreateRelationship.Source")>
      canCreateRelationshipSource
      <srs.EnumMember(Value:="canCreateRelationship.Target")>
      canCreateRelationshipTarget
      <srs.EnumMember(Value:="canGetProperties.Object")>
      canGetPropertiesObject
      <srs.EnumMember(Value:="canViewContent.Object")>
      canViewContentObject
      <srs.EnumMember(Value:="canUpdateProperties.Object")>
      canUpdatePropertiesObject
      <srs.EnumMember(Value:="canMove.Object")>
      canMoveObject
      <srs.EnumMember(Value:="canMove.Target")>
      canMoveTarget
      <srs.EnumMember(Value:="canMove.Source")>
      canMoveSource
      <srs.EnumMember(Value:="canDelete.Object")>
      canDeleteObject
      <srs.EnumMember(Value:="canDeleteTree.Folder")>
      canDeleteTreeFolder
      <srs.EnumMember(Value:="canSetContent.Document")>
      canSetContentDocument
      <srs.EnumMember(Value:="canDeleteContent.Document")>
      canDeleteContentDocument
      <srs.EnumMember(Value:="canAddToFolder.Object")>
      canAddToFolderObject
      <srs.EnumMember(Value:="canAddToFolder.Folder")>
      canAddToFolderFolder
      <srs.EnumMember(Value:="canRemoveFromFolder.Object")>
      canRemoveFromFolderObject
      <srs.EnumMember(Value:="canRemoveFromFolder.Folder")>
      canRemoveFromFolderFolder
      <srs.EnumMember(Value:="canCheckout.Document")>
      canCheckoutDocument
      <srs.EnumMember(Value:="canCancelCheckout.Document")>
      canCancelCheckoutDocument
      <srs.EnumMember(Value:="canCheckin.Document")>
      canCheckinDocument
      <srs.EnumMember(Value:="canGetAllVersions.VersionSeries")>
      canGetAllVersionsVersionSeries
      <srs.EnumMember(Value:="canGetObjectRelationships.Object")>
      canGetObjectRelationshipsObject
      <srs.EnumMember(Value:="canAddPolicy.Object")>
      canAddPolicyObject
      <srs.EnumMember(Value:="canAddPolicy.Policy")>
      canAddPolicyPolicy
      <srs.EnumMember(Value:="canRemovePolicy.Object")>
      canRemovePolicyObject
      <srs.EnumMember(Value:="canRemovePolicy.Policy")>
      canRemovePolicyPolicy
      <srs.EnumMember(Value:="canGetAppliedPolicies.Object")>
      canGetAppliedPoliciesObject
      <srs.EnumMember(Value:="canGetACL.Object")>
      canGetACLObject
      <srs.EnumMember(Value:="canApplyACL.Object")>
      canApplyACLObject
   End Enum

   Public Enum enumBaseObjectTypeIds As Integer
      <srs.EnumMember(Value:="cmis:document")>
      cmisDocument
      <srs.EnumMember(Value:="cmis:folder")>
      cmisFolder
      <srs.EnumMember(Value:="cmis:relationship")>
      cmisRelationship
      <srs.EnumMember(Value:="cmis:policy")>
      cmisPolicy
      <srs.EnumMember(Value:="cmis:item")>
      cmisItem
      <srs.EnumMember(Value:="cmis:secondary")>
      cmisSecondary
   End Enum

   Public Enum enumBasicPermissions As Integer
      <srs.EnumMember(Value:="cmis:read")>
      cmisRead
      <srs.EnumMember(Value:="cmis:write")>
      cmisWrite
      <srs.EnumMember(Value:="cmis:all")>
      cmisAll
   End Enum

   Public Enum enumCapabilityACL As Integer
      none
      discover
      manage
   End Enum

   Public Enum enumCapabilityChanges As Integer
      none
      objectidsonly
      properties
      all
   End Enum

   Public Enum enumCapabilityContentStreamUpdates As Integer
      anytime
      pwconly
      none
   End Enum

   Public Enum enumCapabilityJoin As Integer
      none
      inneronly
      innerandouter
   End Enum

   Public Enum enumCapabilityOrderBy As Integer
      none
      common
      custom
   End Enum

   Public Enum enumCapabilityQuery As Integer
      none
      metadataonly
      fulltextonly
      bothseparate
      bothcombined
   End Enum

   Public Enum enumCapabilityRendition As Integer
      none
      read
   End Enum

   Public Enum enumCardinality As Integer
      [single]
      multi
   End Enum

   Public Enum enumContentStreamAllowed As Integer
      notallowed
      allowed
      required
   End Enum

   Public Enum enumDateTimeResolution As Integer
      year
      [date]
      time
   End Enum

   Public Enum enumDecimalPrecision As xs_Integer
      <srs.EnumMember(Value:="32")>
      [single] = 32
      <srs.EnumMember(Value:="64")>
      [double] = 64
   End Enum

   Public Enum enumIncludeRelationships As Integer
      none
      source
      target
      both
   End Enum

   Public Enum enumPropertiesBase As Integer
      <srs.EnumMember(Value:="cmis:name")>
      cmisName
      <srs.EnumMember(Value:="cmis:description")>
      cmisDescription
      <srs.EnumMember(Value:="cmis:objectId")>
      cmisObjectId
      <srs.EnumMember(Value:="cmis:objectTypeId")>
      cmisObjectTypeId
      <srs.EnumMember(Value:="cmis:baseTypeId")>
      cmisBaseTypeId
      <srs.EnumMember(Value:="cmis:secondaryObjectTypeIds")>
      cmisSecondaryObjectTypeIds
      <srs.EnumMember(Value:="cmis:createdBy")>
      cmisCreatedBy
      <srs.EnumMember(Value:="cmis:creationDate")>
      cmisCreationDate
      <srs.EnumMember(Value:="cmis:lastModifiedBy")>
      cmisLastModifiedBy
      <srs.EnumMember(Value:="cmis:lastModificationDate")>
      cmisLastModificationDate
      <srs.EnumMember(Value:="cmis:changeToken")>
      cmisChangeToken
   End Enum

   Public Enum enumPropertiesDocument As Integer
      <srs.EnumMember(Value:="cmis:isImmutable")>
      cmisIsImmutable
      <srs.EnumMember(Value:="cmis:isLatestVersion")>
      cmisIsLatestVersion
      <srs.EnumMember(Value:="cmis:isMajorVersion")>
      cmisIsMajorVersion
      <srs.EnumMember(Value:="cmis:isLatestMajorVersion")>
      cmisIsLatestMajorVersion
      <srs.EnumMember(Value:="cmis:isPrivateWorkingCopy")>
      cmisIsPrivateWorkingCopy
      <srs.EnumMember(Value:="cmis:versionLabel")>
      cmisVersionLabel
      <srs.EnumMember(Value:="cmis:versionSeriesId")>
      cmisVersionSeriesId
      <srs.EnumMember(Value:="cmis:isVersionSeriesCheckedOut")>
      cmisIsVersionSeriesCheckedOut
      <srs.EnumMember(Value:="cmis:versionSeriesCheckedOutBy")>
      cmisVersionSeriesCheckedOutBy
      <srs.EnumMember(Value:="cmis:versionSeriesCheckedOutId")>
      cmisVersionSeriesCheckedOutId
      <srs.EnumMember(Value:="cmis:checkinComment")>
      cmisCheckinComment
      <srs.EnumMember(Value:="cmis:contentStreamLength")>
      cmisContentStreamLength
      <srs.EnumMember(Value:="cmis:contentStreamMimeType")>
      cmisContentStreamMimeType
      <srs.EnumMember(Value:="cmis:contentStreamFileName")>
      cmisContentStreamFileName
      <srs.EnumMember(Value:="cmis:contentStreamId")>
      cmisContentStreamId
   End Enum

   Public Enum enumPropertiesFolder As Integer
      <srs.EnumMember(Value:="cmis:parentId")>
      cmisParentId
      <srs.EnumMember(Value:="cmis:allowedChildObjectTypeIds")>
      cmisAllowedChildObjectTypeIds
      <srs.EnumMember(Value:="cmis:path")>
      cmisPath
   End Enum

   Public Enum enumPropertiesPolicy As Integer
      <srs.EnumMember(Value:="cmis:policyText")>
      cmisPolicyText
   End Enum

   Public Enum enumPropertiesRelationship As Integer
      <srs.EnumMember(Value:="cmis:sourceId")>
      cmisSourceId
      <srs.EnumMember(Value:="cmis:targetId")>
      cmisTargetId
   End Enum

   Public Enum enumPropertyType As Integer
      [boolean]
      id
      [integer]
      datetime
      [decimal]
      html
      [string]
      uri
   End Enum

   Public Enum enumRelationshipDirection As Integer
      source
      target
      either
   End Enum

   Public Enum enumRenditionKind As Integer
      <srs.EnumMember(Value:="cmis:thumbnail")>
      cmisThumbnail
   End Enum

   Public Enum enumSupportedPermissions As Integer
      basic
      repository
      both
   End Enum

   Public Enum enumTypeOfChanges As Integer
      created
      updated
      deleted
      security
   End Enum

   Public Enum enumUnfileObject As Integer
      unfile
      deletesinglefiled
      delete
   End Enum

   Public Enum enumUpdatability As Integer
      [readonly]
      readwrite
      whencheckedout
      oncreate
   End Enum

   Public Enum enumUsers As Integer
      <srs.EnumMember(Value:="cmis:user")>
      cmisUser
   End Enum

   Public Enum enumVersioningState As Integer
      none
      checkedout
      minor
      major
   End Enum
End Namespace
