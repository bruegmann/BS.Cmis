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
Imports cc = CmisObjectModel.Client
Imports ccg = CmisObjectModel.Client.Generic
Imports requests = CmisObjectModel.Messaging.Requests
Imports responses = CmisObjectModel.Messaging.Responses
Imports ss = System.ServiceModel
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml

Namespace CmisObjectModel.Contracts
   Public Interface ICmisClient

#Region "Repository"
      ''' <summary>
      ''' Creates a new type definition that is a subtype of an existing specified parent type
      ''' </summary>
      ''' <remarks></remarks>
      Function CreateType(request As requests.createType) As ccg.Response(Of responses.createTypeResponse)

      ''' <summary>
      ''' Deletes a type definition
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function DeleteType(request As requests.deleteType) As ccg.Response(Of responses.deleteTypeResponse)

      ''' <summary>
      ''' Returns all repositories
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetRepositories(request As requests.getRepositories) As ccg.Response(Of responses.getRepositoriesResponse)

      ''' <summary>
      ''' Returns the workspace of specified repository or null
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetRepositoryInfo(request As requests.getRepositoryInfo, Optional ignoreCache As Boolean = False) As ccg.Response(Of responses.getRepositoryInfoResponse)

      ''' <summary>
      ''' Returns the list of object-types defined for the repository that are children of the specified type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetTypeChildren(request As requests.getTypeChildren) As ccg.Response(Of responses.getTypeChildrenResponse)

      ''' <summary>
      ''' Gets the definition of the specified object-type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetTypeDefinition(request As requests.getTypeDefinition) As ccg.Response(Of responses.getTypeDefinitionResponse)

      ''' <summary>
      ''' Returns the set of the descendant object-types defined for the Repository under the specified type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetTypeDescendants(request As requests.getTypeDescendants) As ccg.Response(Of responses.getTypeDescendantsResponse)

      ''' <summary>
      ''' Updates a type definition
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function UpdateType(request As requests.updateType) As ccg.Response(Of responses.updateTypeResponse)
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Gets the list of documents that are checked out that the user has access to
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetCheckedOutDocs(request As requests.getCheckedOutDocs) As ccg.Response(Of responses.getCheckedOutDocsResponse)

      ''' <summary>
      ''' Gets the list of child objects contained in the specified folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetChildren(request As requests.getChildren) As ccg.Response(Of responses.getChildrenResponse)

      ''' <summary>
      ''' Gets the set of descendant objects containded in the specified folder or any of its child-folders
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetDescendants(request As requests.getDescendants) As ccg.Response(Of responses.getDescendantsResponse)

      ''' <summary>
      ''' Gets the parent folder object for the specified folder object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetFolderParent(request As requests.getFolderParent) As ccg.Response(Of responses.getFolderParentResponse)

      ''' <summary>
      ''' Gets the set of descendant folder objects contained in the specified folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetFolderTree(request As requests.getFolderTree) As ccg.Response(Of responses.getFolderTreeResponse)

      ''' <summary>
      ''' Gets the parent folder(s) for the specified fileable object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetObjectParents(request As requests.getObjectParents) As ccg.Response(Of responses.getObjectParentsResponse)
#End Region

#Region "Object"
      ''' <summary>
      ''' Appends to the content stream for the specified document object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function AppendContentStream(request As requests.appendContentStream) As ccg.Response(Of responses.appendContentStreamResponse)

      ''' <summary>
      ''' Updates properties and secondary types of one or more objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function BulkUpdateProperties(request As requests.bulkUpdateProperties) As ccg.Response(Of responses.bulkUpdatePropertiesResponse)

      ''' <summary>
      ''' Creates a document object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateDocument(request As requests.createDocument) As ccg.Response(Of responses.createDocumentResponse)

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateDocumentFromSource(request As requests.createDocumentFromSource) As ccg.Response(Of responses.createDocumentFromSourceResponse)

      ''' <summary>
      ''' Creates a folder object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateFolder(request As requests.createFolder) As ccg.Response(Of responses.createFolderResponse)

      ''' <summary>
      ''' Creates a item object of the specified type (given by the cmis:objectTypeId property) in the specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateItem(request As requests.createItem) As ccg.Response(Of responses.createItemResponse)

      ''' <summary>
      ''' Creates a policy object of the specified type (given by the cmis:objectTypeId property) in the specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreatePolicy(request As requests.createPolicy) As ccg.Response(Of responses.createPolicyResponse)

      ''' <summary>
      ''' Creates a relationship object of the specified type (given by the cmis:objectTypeId property)
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateRelationship(request As requests.createRelationship) As ccg.Response(Of responses.createRelationshipResponse)

      ''' <summary>
      ''' Deletes the content stream for the specified document object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function DeleteContentStream(request As requests.deleteContentStream) As ccg.Response(Of responses.deleteContentStreamResponse)

      ''' <summary>
      ''' Deletes the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function DeleteObject(request As requests.deleteObject) As ccg.Response(Of responses.deleteObjectResponse)

      ''' <summary>
      ''' Deletes the specified folder object and all of its child- and descendant-objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function DeleteTree(request As requests.deleteTree) As ccg.Response(Of responses.deleteTreeResponse)

      ''' <summary>
      ''' Gets the list of allowable actions for an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetAllowableActions(request As requests.getAllowableActions) As ccg.Response(Of responses.getAllowableActionsResponse)

      ''' <summary>
      ''' Gets the content stream for the specified document object, or gets a rendition stream for a specified rendition of a document or folder object.
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetContentStream(request As requests.getContentStream) As ccg.Response(Of responses.getContentStreamResponse)

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetObject(request As requests.getObject) As ccg.Response(Of responses.getObjectResponse)

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetObjectByPath(request As requests.getObjectByPath) As ccg.Response(Of responses.getObjectByPathResponse)

      ''' <summary>
      ''' Gets the list of properties for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetProperties(request As requests.getProperties) As ccg.Response(Of responses.getPropertiesResponse)

      ''' <summary>
      ''' Gets the list of associated renditions for the specified object. Only rendition attributes are returned, not rendition stream
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetRenditions(request As requests.getRenditions) As ccg.Response(Of responses.getRenditionsResponse)

      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function MoveObject(request As requests.moveObject) As ccg.Response(Of responses.moveObjectResponse)

      ''' <summary>
      ''' Sets the content stream for the specified document object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function SetContentStream(request As requests.setContentStream) As ccg.Response(Of responses.setContentStreamResponse)

      ''' <summary>
      ''' Updates properties and secondary types of the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function UpdateProperties(request As requests.updateProperties) As ccg.Response(Of responses.updatePropertiesResponse)
#End Region

#Region "Multi"
      ''' <summary>
      ''' Adds an existing fileable non-folder object to a folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function AddObjectToFolder(request As requests.addObjectToFolder) As ccg.Response(Of responses.addObjectToFolderResponse)

      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function RemoveObjectFromFolder(request As requests.removeObjectFromFolder) As ccg.Response(Of responses.removeObjectFromFolderResponse)
#End Region

#Region "Discovery"
      ''' <summary>
      ''' Gets a list of content changes. This service is intended to be used by search crawlers or other applications that need to
      ''' efficiently understand what has changed in the repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetContentChanges(request As requests.getContentChanges) As ccg.Response(Of responses.getContentChangesResponse)

      ''' <summary>
      ''' Executes a CMIS query statement against the contents of the repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function Query(request As requests.query) As ccg.Response(Of responses.queryResponse)
#End Region

#Region "Versioning"
      ''' <summary>
      ''' Reverses the effect of a check-out (checkOut). Removes the Private Working Copy of the checked-out document, allowing other documents
      ''' in the version series to be checked out again. If the private working copy has been created by createDocument, cancelCheckOut MUST
      ''' delete the created document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CancelCheckOut(request As requests.cancelCheckOut) As ccg.Response(Of responses.cancelCheckOutResponse)

      ''' <summary>
      ''' Checks-in the Private Working Copy document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CheckIn(request As requests.checkIn) As ccg.Response(Of responses.checkInResponse)

      ''' <summary>
      ''' Create a private working copy (PWC) of the document
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CheckOut(request As requests.checkOut) As ccg.Response(Of responses.checkOutResponse)

      ''' <summary>
      ''' Returns the list of all document objects in the specified version series, sorted by cmis:creationDate descending
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetAllVersions(request As requests.getAllVersions) As ccg.Response(Of responses.getAllVersionsResponse)

      ''' <summary>
      ''' Get the latest document object in the version series
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetObjectOfLatestVersion(request As requests.getObjectOfLatestVersion) As ccg.Response(Of responses.getObjectOfLatestVersionResponse)

      ''' <summary>
      ''' Get a subset of the properties for the latest document object in the version series
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetPropertiesOfLatestVersion(request As requests.getPropertiesOfLatestVersion) As ccg.Response(Of responses.getPropertiesOfLatestVersionResponse)
#End Region

#Region "Relationship"
      ''' <summary>
      ''' Gets all or a subset of relationships associated with an independent object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetObjectRelationships(request As requests.getObjectRelationships) As ccg.Response(Of responses.getObjectRelationshipsResponse)
#End Region

#Region "Policy"
      ''' <summary>
      ''' Applies a specified policy to an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function ApplyPolicy(request As requests.applyPolicy) As ccg.Response(Of responses.applyPolicyResponse)

      ''' <summary>
      ''' Gets the list of policies currently applied to the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetAppliedPolicies(request As requests.getAppliedPolicies) As ccg.Response(Of responses.getAppliedPoliciesResponse)

      ''' <summary>
      ''' Removes a specified policy from an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function RemovePolicy(request As requests.removePolicy) As ccg.Response(Of responses.removePolicyResponse)
#End Region

#Region "Acl"
      ''' <summary>
      ''' Adds or removes the given ACEs to or from the ACL of an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function ApplyAcl(request As requests.applyACL) As ccg.Response(Of responses.applyACLResponse)

      ''' <summary>
      ''' Get the ACL currently applied to the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetAcl(request As requests.getACL) As ccg.Response(Of responses.getACLResponse)
#End Region

#Region "Mapping"
      ''' <summary>
      ''' Adds mapping information to the client
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="mapper"></param>
      ''' <remarks></remarks>
      Sub AddMapper(repositoryId As String, mapper As Data.Mapper)

      ''' <summary>
      ''' Removes mapping information from the client
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks></remarks>
      Sub RemoveMapper(repositoryId As String)
#End Region

      ''' <summary>
      ''' Authentication information used for all requests
      ''' </summary>
      ''' <returns></returns>
      ReadOnly Property Authentication As AuthenticationInfo

      ''' <summary>
      ''' ClientType (AtomPubBinding, BrowserBinding)
      ''' </summary>
      ''' <returns></returns>
      ReadOnly Property ClientType As enumClientType

      ''' <summary>
      ''' Timeout HttpWebRequest.Timeout. If not set default is used.
      ''' </summary>
      ''' <returns></returns>
      ReadOnly Property ConnectTimeout As Integer?

      ''' <summary>
      ''' Returns the uri to get the content of a cmisDocument
      ''' </summary>
      Function GetContentStreamLink(repositoryId As String, objectId As String, Optional streamId As String = Nothing) As ccg.Response(Of String)

      Sub Logout(repositoryId As String)
      Sub Ping(repositoryId As String)

      ''' <summary>
      ''' Timeout read or write operations (HttpWebRequest.ReadWriteTimeout). If not set default is used.
      ''' </summary>
      ''' <returns></returns>
      ReadOnly Property ReadWriteTimeout As Integer?

      ''' <summary>
      ''' Installs an automatic ping to tell the server that the client is still alive
      ''' </summary>
      ''' <param name="interval">Time-interval in seconds</param>
      ''' <remarks></remarks>
      Sub RegisterPing(repositoryId As String, interval As Double)

      ''' <summary>
      ''' The base address of the cmis-service the client is connected with
      ''' </summary>
      ''' <returns></returns>
      ReadOnly Property ServiceDocUri As Uri

      ''' <summary>
      ''' Returns True if the binding supports a succinct representation of properties
      ''' </summary>
      ReadOnly Property SupportsSuccinct As Boolean
      ''' <summary>
      ''' Returns True if the binding supports token parameters
      ''' </summary>
      ReadOnly Property SupportsToken As Boolean

      ''' <summary>
      ''' Timeout of request in milliseconds; if not defined, the default value of System.Net.HttpWebRequest.Timeout is used (100s)
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Property Timeout As Integer?

      ''' <summary>
      ''' Removes the automatic ping
      ''' </summary>
      Sub UnregisterPing(repositoryId As String)

      ReadOnly Property User As String

      ReadOnly Property Vendor As CmisObjectModel.Client.Vendors.Vendor

   End Interface
End Namespace