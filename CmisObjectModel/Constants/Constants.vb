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

Namespace CmisObjectModel.Constants
   Public MustInherit Class CmisPredefinedPropertyNames
      Private Sub New()
      End Sub

      Public Const AllowedChildObjectTypeIds As String = "cmis:allowedChildObjectTypeIds"
      Public Const BaseTypeId As String = "cmis:baseTypeId"
      Public Const ChangeToken As String = "cmis:changeToken"
      Public Const CheckinComment As String = "cmis:checkinComment"
      Public Const ContentStreamFileName As String = "cmis:contentStreamFileName"
      Public Const ContentStreamId As String = "cmis:contentStreamId"
      Public Const ContentStreamLength As String = "cmis:contentStreamLength"
      Public Const ContentStreamMimeType As String = "cmis:contentStreamMimeType"
      Public Const CreatedBy As String = "cmis:createdBy"
      Public Const CreationDate As String = "cmis:creationDate"
      Public Const Description As String = "cmis:description"
      Public Const IsImmutable As String = "cmis:isImmutable"
      Public Const IsLatestMajorVersion As String = "cmis:isLatestMajorVersion"
      Public Const IsLatestVersion As String = "cmis:isLatestVersion"
      Public Const IsMajorVersion As String = "cmis:isMajorVersion"
      Public Const IsPrivateWorkingCopy As String = "cmis:isPrivateWorkingCopy"
      Public Const IsVersionSeriesCheckedOut As String = "cmis:isVersionSeriesCheckedOut"
      Public Const LastModificationDate As String = "cmis:lastModificationDate"
      Public Const LastModifiedBy As String = "cmis:lastModifiedBy"
      Public Const Name As String = "cmis:name"
      Public Const ObjectId As String = "cmis:objectId"
      Public Const ObjectTypeId As String = "cmis:objectTypeId"
      Public Const ParentId As String = "cmis:parentId"
      Public Const Path As String = "cmis:path"
      Public Const PolicyText As String = "cmis:policyText"
      Public Const SecondaryObjectTypeIds As String = "cmis:secondaryObjectTypeIds"
      Public Const SourceId As String = "cmis:sourceId"
      Public Const TargetId As String = "cmis:targetId"
      Public Const VersionLabel As String = "cmis:versionLabel"
      Public Const VersionSeriesCheckedOutBy As String = "cmis:versionSeriesCheckedOutBy"
      Public Const VersionSeriesCheckedOutId As String = "cmis:versionSeriesCheckedOutId"
      Public Const VersionSeriesId As String = "cmis:versionSeriesId"

      Public Const RM_DestructionDate As String = "cmis:rm_destructionDate"
      Public Const RM_ExpirationDate As String = "cmis:rm_expirationDate"
      Public Const RM_HoldIds As String = "cmis:rm_holdIds"
      Public Const RM_StartOfRetention As String = "cmis:rm_startOfRetention"

      ''' <summary>
      ''' Extensions defined in this implementation
      ''' </summary>
      ''' <remarks></remarks>
      Public MustInherit Class Extensions
         Private Sub New()
         End Sub

         Public Const ForeignChangeToken As String = "com:foreignChangeToken"
         Public Const ForeignObjectId As String = "com:foreignObjectId"
         Public Const LastSyncError As String = "com:lastSyncError"
         Public Const SyncRequired As String = "com:syncRequired"
      End Class
   End Class

   Public MustInherit Class CollectionInfos
      Private Sub New()
      End Sub

      Public Const CheckedOut As String = "checkedout"
      Public Const Policies As String = "policies"
      Public Const Query As String = "query"
      Public Const Relationships As String = "relationships"
      Public Const Root As String = "root"
      Public Const Types As String = "types"
      Public Const Unfiled As String = "unfiled"
      Public Const Update As String = "update"
   End Class

   ''' <summary>
   ''' CmisObjectModel specific extended properties of XmlSerializable-classes
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class ExtendedProperties
      Public Const Cardinality As String = "{53c6a1e8-eca2-48cb-aa7e-dc843ccb682f}"
      Public Const DeclaringType As String = "{d2d3cd23-1ce8-4f7c-b6f4-bd99305b04eb}"
   End Class

   Public MustInherit Class MediaTypes
      Private Sub New()
      End Sub

      Public Const Acl As String = "application/cmisacl+xml"
      Public Const AllowableActions As String = "application/cmisallowableactions+xml"
      Public Const Entry As String = "application/atom+xml;type=entry"
      Public Const Feed As String = "application/atom+xml;type=feed"
      Public Const Html As String = "text/html"
      Public Const JavaScript As String = "application/javascript"
      Public Const Json As String = "application/json"
      Public Const JsonText As String = "text/json"
      Public Const MultipartFormData As String = "multipart/form-data"
      Public Const PlainText As String = "text/plain"
      Public Const Query As String = "application/cmisquery+xml"
      Public Const Request As String = "application/cmisatom+xml;type=request"
      Public Const Service As String = "application/atomsvc+xml"
      Public Const Stream As String = "application/octet-stream"
      Public Const Tree As String = "application/cmistree+xml"
      Public Const UrlEncoded As String = "application/x-www-form-urlencoded"
      Public Const UrlEncodedUTF8 As String = "application/x-www-form-urlencoded; charset=UTF-8"
      Public Const Xml As String = "application/cmisatom+xml"
      Public Const XmlApplication = "application/xml"
      Public Const XmlText As String = "text/xml"
   End Class

   Public MustInherit Class Namespaces
      Private Sub New()
      End Sub

      Public Const alf As String = "http://www.alfresco.org"
      Public Const app As String = "http://www.w3.org/2007/app"
      Public Const atom As String = "http://www.w3.org/2005/Atom"
      Public Const atompub As String = app
      Public Const browser As String = "http://docs.oasis-open.org/ns/cmis/browser/201103/"
      Public Const cmis As String = "http://docs.oasis-open.org/ns/cmis/core/200908/"
      Public Const com As String = "http://bruegmann-software.de/cmisObjectModel"
      Public Const cmisl As String = "http://docs.oasis-open.org/ns/cmis/link/200908/"
      Public Const cmism As String = "http://docs.oasis-open.org/ns/cmis/messaging/200908/"
      Public Const cmisra As String = "http://docs.oasis-open.org/ns/cmis/restatom/200908/"
      Public Const cmisw As String = "http://docs.oasis-open.org/ns/cmis/ws/200908/"
      Public Const w3instance As String = "http://www.w3.org/2001/XMLSchema-instance"
      Public Const xmlns As String = "http://www.w3.org/2000/xmlns/"
   End Class

   Public MustInherit Class NamespacesLowerInvariant
      Private Sub New()
      End Sub

      Public Shared ReadOnly alf As String = Namespaces.alf.ToLowerInvariant()
      Public Shared ReadOnly app As String = Namespaces.app.ToLowerInvariant()
      Public Shared ReadOnly atom As String = Namespaces.atom.ToLowerInvariant()
      Public Shared ReadOnly atompub As String = Namespaces.atompub.ToLowerInvariant()
      Public Shared ReadOnly browser As String = Namespaces.browser.ToLowerInvariant()
      Public Shared ReadOnly cmis As String = Namespaces.cmis.ToLowerInvariant()
      Public Shared ReadOnly com As String = Namespaces.com.ToLowerInvariant()
      Public Shared ReadOnly cmisl As String = Namespaces.cmisl.ToLowerInvariant()
      Public Shared ReadOnly cmism As String = Namespaces.cmism.ToLowerInvariant()
      Public Shared ReadOnly cmisra As String = Namespaces.cmisra.ToLowerInvariant()
      Public Shared ReadOnly cmisw As String = Namespaces.cmisw.ToLowerInvariant()
      Public Shared ReadOnly w3instance As String = Namespaces.w3instance.ToLowerInvariant()
      Public Shared ReadOnly xmlns As String = Namespaces.xmlns.ToLowerInvariant()

   End Class

   Public MustInherit Class UriTemplates
      Private Sub New()
      End Sub

      Public Const ObjectById As String = "objectbyid"
      Public Const ObjectByPath As String = "objectbypath"
      Public Const Query As String = "query"
      Public Const TypeById As String = "typebyid"
   End Class
End Namespace