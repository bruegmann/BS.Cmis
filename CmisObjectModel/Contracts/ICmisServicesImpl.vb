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
Imports cc = CmisObjectModel.Core
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports ccg = CmisObjectModel.Common.Generic
Imports cm = CmisObjectModel.Messaging
Imports cs = CmisObjectModel.ServiceModel
Imports ss = System.ServiceModel
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Contracts
   ''' <summary>
   ''' Defines the custom implementation of cmis
   ''' </summary>
   ''' <remarks></remarks>
   Public Interface ICmisServicesImpl

#Region "Repository"
      ''' <summary>
      ''' Creates a new type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="newType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateType(repositoryId As String, newType As ccdt.cmisTypeDefinitionType) As ccg.Result(Of ccdt.cmisTypeDefinitionType)

      ''' <summary>
      ''' Deletes a type definition
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="typeId">TypeId</param>
      ''' <remarks></remarks>
      Function DeleteType(repositoryId As String, typeId As String) As Exception

      ''' <summary>
      ''' Returns a list of workspaces for all available repositories
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetRepositories() As ccg.Result(Of Core.cmisRepositoryInfoType())
      ''' <summary>
      ''' Returns a workspace for the specified repository
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <returns></returns>
      ''' <remarks>log in into specified respository</remarks>
      Function GetRepositoryInfo(repositoryId As String) As ccg.Result(Of Core.cmisRepositoryInfoType)

      ''' <summary>
      ''' Returns all child types of the specified type, if defined, otherwise the basetypes of the repository.
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="typeId">TypeId; optional
      ''' If specified, then the repository MUST return all of child types of the specified type
      ''' If not specified, then the repository MUST return all base object-types</param>
      ''' <param name="includePropertyDefinitions">If TRUE, then the repository MUST return the property deﬁnitions for each object-type.
      ''' If FALSE (default), the repository MUST return only the attributes for each object-type</param>
      ''' <param name="maxItems">optional</param>
      ''' <param name="skipCount">optional</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetTypeChildren(repositoryId As String, typeId As String,
                               includePropertyDefinitions As Boolean, maxItems As xs_Integer?, skipCount As xs_Integer?) As ccg.Result(Of cm.cmisTypeDefinitionListType)

      ''' <summary>
      ''' Returns the descendant object-types under the specified type.
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="typeId">TypeId; optional
      ''' If speciﬁed, then the repository MUST return all of descendant types of the speciﬁed type
      ''' If not speciﬁed, then the Repository MUST return all types and MUST ignore the value of the depth parameter</param>
      ''' <param name="includePropertyDefinitions">If TRUE, then the repository MUST return the property deﬁnitions for each object-type.
      ''' If FALSE (default), the repository MUST return only the attributes for each object-type</param>
      ''' <param name="depth">The number of levels of depth in the type hierarchy from which to return results. Valid values are
      ''' 1:  Return only types that are children of the type. See also getTypeChildren
      ''' >1: Return only types that are children of the type and descendants up to [depth] levels deep
      ''' -1: Return ALL descendant types at all depth levels in the CMIS hierarchy</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetTypeDescendants(repositoryId As String, typeId As String, includePropertyDefinitions As Boolean, depth As xs_Integer?) As ccg.Result(Of cm.cmisTypeContainer)

      ''' <summary>
      ''' Returns the type-definition of the specified type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetTypeDefinition(repositoryId As String, typeId As String) As ccg.Result(Of ccdt.cmisTypeDefinitionType)

      ''' <summary>
      ''' Explicit log in into repository
      ''' </summary>
      Function Login(repositoryId As String, authorization As String) As ccg.Result(Of System.Net.HttpStatusCode)

      ''' <summary>
      ''' Log out from repository
      ''' </summary>
      ''' <remarks>Not defined in the CMIS-specification</remarks>
      Function Logout(repositoryId As String) As ccg.Result(Of System.Net.HttpStatusCode)

      ''' <summary>
      ''' Tell server that the client is alive
      ''' </summary>
      ''' <remarks>Not defined in the CMIS-specification</remarks>
      Function Ping(repositoryId As String) As ccg.Result(Of System.Net.HttpStatusCode)

      ''' <summary>
      ''' Updates a type definition
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="modifiedType">A type definition object with the property definitions that are to change.
      ''' Repositories MUST ignore all fields in the type definition except for the type id and the list of properties.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function UpdateType(repositoryId As String, modifiedType As ccdt.cmisTypeDefinitionType) As ccg.Result(Of ccdt.cmisTypeDefinitionType)
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Returns a list of check out object the user has access to.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="filter"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="skipCount"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' The following CMIS Atom extension element MUST be included inside the atom entries:
      ''' cmisra:object inside atom:entry
      ''' The following CMIS Atom extension element MAY be included inside the atom feed:
      ''' cmisra:numItems
      ''' </remarks>
      Function GetCheckedOutDocs(repositoryId As String, folderId As String, filter As String,
                                 maxItems As xs_Integer?, skipCount As xs_Integer?, renditionFilter As String,
                                 includeAllowableActions As Boolean?, includeRelationships As Core.enumIncludeRelationships?) As ccg.Result(Of cs.cmisObjectListType)

      ''' <summary>
      ''' Returns all children of the specified CMIS object.
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="folderId">The identifier for the folder</param>
      ''' <param name="maxItems"></param>
      ''' <param name="skipCount"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="orderBy"></param>
      ''' <param name="includePathSegment">If TRUE, returns a PathSegment for each child object for use in constructing that object’s path.
      ''' Defaults to FALSE</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetChildren(repositoryId As String, folderId As String,
                           maxItems As xs_Integer?, skipCount As xs_Integer?, filter As String,
                           includeAllowableActions As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                           renditionFilter As String, orderBy As String, includePathSegment As Boolean) As ccg.Result(Of cs.cmisObjectInFolderListType)

      ''' <summary>
      ''' Returns the descendant objects contained in the specified folder or any of its child-folders.
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="folderId">The identifier for the folder</param>
      ''' <param name="filter"></param>
      ''' <param name="depth">The number of levels of depth in the type hierarchy from which to return results. Valid values are
      ''' 1:  Return only types that are children of the type. See also getTypeChildren
      ''' >1: Return only types that are children of the type and descendants up to [depth] levels deep
      ''' -1: Return ALL descendant types at all depth levels in the CMIS hierarchy</param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includePathSegment"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetDescendants(repositoryId As String, folderId As String, filter As String, depth As xs_Integer?,
                              includeAllowableActions As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                              renditionFilter As String, includePathSegment As Boolean) As ccg.Result(Of cs.cmisObjectInFolderContainerType)

      ''' <summary>
      ''' Returns the descendant folders contained in the specified folder
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="folderId">The identifier for the folder</param>
      ''' <param name="filter"></param>
      ''' <param name="depth">The number of levels of depth in the type hierarchy from which to return results. Valid values are
      ''' 1:  Return only types that are children of the type. See also getTypeChildren
      ''' >1: Return only types that are children of the type and descendants up to [depth] levels deep
      ''' -1: Return ALL descendant types at all depth levels in the CMIS hierarchy</param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="includePathSegment">If TRUE, returns a PathSegment for each child object for use in constructing that object’s path.
      ''' Defaults to FALSE</param>
      ''' <param name="renditionFilter"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetFolderTree(repositoryId As String, folderId As String, filter As String, depth As xs_Integer?,
                             includeAllowableActions As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                             includePathSegment As Boolean, renditionFilter As String) As ccg.Result(Of cs.cmisObjectInFolderContainerType)

      ''' <summary>
      ''' Returns the parent folder-object of the specified folder
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="folderId"></param>
      ''' <param name="filter"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetFolderParent(repositoryId As String, folderId As String, filter As String) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Returns the parent folders for the specified object
      ''' </summary>
      ''' <param name="repositoryId">The identifier for the repository</param>
      ''' <param name="objectId"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeRelativePathSegment"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' This feed contains a set of atom entries for each parent of the object that MUST contain: 
      ''' cmisra:object inside atom:entry 
      ''' cmisra:relativePathSegment 
      ''' </remarks>
      Function GetObjectParents(repositoryId As String, objectId As String, filter As String, includeAllowableActions As Boolean?,
                                includeRelationships As Core.enumIncludeRelationships?,
                                renditionFilter As String, includeRelativePathSegment As Boolean?) As ccg.Result(Of cs.cmisObjectParentsType())

      ''' <summary>
      ''' Returns a list of check out object the user has access to.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="skipCount"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="orderBy"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetUnfiledObjects(repositoryId As String, maxItems As xs_Integer?, skipCount As xs_Integer?,
                                 filter As String, includeAllowableActions As Boolean?,
                                 includeRelationships As Core.enumIncludeRelationships?,
                                 renditionFilter As String, orderBy As String) As ccg.Result(Of cs.cmisObjectListType)
#End Region

#Region "Object"
      ''' <summary>
      ''' Appends to the content stream for the specified document object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="contentStream"></param>
      ''' <param name="isLastChunk"></param>
      ''' <param name="changeToken"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function AppendContentStream(repositoryId As String, objectId As String, contentStream As IO.Stream, mimeType As String, fileName As String,
                                   isLastChunk As Boolean, changeToken As String) As ccg.Result(Of Messaging.Responses.setContentStreamResponse)

      ''' <summary>
      ''' Updates properties and secondary types of one or more objects
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="data"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function BulkUpdateProperties(repositoryId As String, data As Core.cmisBulkUpdateType) As ccg.Result(Of cs.cmisObjectListType)

      ''' <summary>
      ''' Creates a new document in the specified folder or as unfiled document
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="newDocument"></param>
      ''' <param name="folderId">If specified, the identifier for the folder that MUST be the parent folder for the newly-created document object.
      ''' This parameter MUST be specified if the repository does NOT support the optional "unfiling" capability.</param>
      ''' <param name="versioningState"></param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created document object, either using the ACL from folderId if specified, or being applied if no folderId is specified.</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created document object, either using the ACL from folderId if specified, or being ignored if no folderId is specified.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateDocument(repositoryId As String, newDocument As Core.cmisObjectType,
                              folderId As String, content As Messaging.cmisContentStreamType,
                              versioningState As Core.enumVersioningState?,
                              addACEs As Core.Security.cmisAccessControlListType,
                              removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) speciﬁed location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="sourceId"></param>
      ''' <param name="properties">The property values that MUST be applied to the object. This list of properties SHOULD only contain properties whose values differ from the source document</param>
      ''' <param name="folderId">If speciﬁed, the identifier for the folder that MUST be the parent folder for the newly-created document object.
      ''' This parameter MUST be specified if the repository does NOT support the optional "unfiling" capability.</param>
      ''' <param name="versioningState"></param>
      ''' <param name="policies">A list of policy ids that MUST be applied to the newly-created document object</param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created document object, either using the ACL from folderId if specified, or being applied if no folderId is specified</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created document object, either using the ACL from folderId if specified, or being ignored if no folderId is specified.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateDocumentFromSource(repositoryId As String, sourceId As String,
                                        properties As Core.Collections.cmisPropertiesType,
                                        folderId As String, versioningState As Core.enumVersioningState?,
                                        policies As String(),
                                        addACEs As Core.Security.cmisAccessControlListType,
                                        removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Creates a folder object of the specified type in the specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="newFolder"></param>
      ''' <param name="parentFolderId">The identifier for the folder that MUST be the parent folder for the newly-created folder object</param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created folder object, either using the ACL from folderId if specified, or being applied if no folderId is specified</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created folder object, either using the ACL from folderId if specified, or being ignored if no folderId is specified</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateFolder(repositoryId As String, newFolder As Core.cmisObjectType,
                            parentFolderId As String,
                            addACEs As Core.Security.cmisAccessControlListType,
                            removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Creates an item object of the specified type in the specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="newItem"></param>
      ''' <param name="folderId">The identifier for the folder that MUST be the parent folder for the newly-created folder object</param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created policy object, either using the ACL from folderId if specified, or being applied if no folderId is specified</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created policy object, either using the ACL from folderId if specified, or being ignored if no folderId is specified</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateItem(repositoryId As String, newItem As Core.cmisObjectType,
                          folderId As String,
                          addACEs As Core.Security.cmisAccessControlListType,
                          removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Creates a policy object of the specified type in the specified location
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="newPolicy"></param>
      ''' <param name="folderId">The identifier for the folder that MUST be the parent folder for the newly-created folder object</param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created policy object, either using the ACL from folderId if specified, or being applied if no folderId is specified</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created policy object, either using the ACL from folderId if specified, or being ignored if no folderId is specified</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreatePolicy(repositoryId As String, newPolicy As Core.cmisObjectType,
                            folderId As String,
                            addACEs As Core.Security.cmisAccessControlListType,
                            removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Creates a relationship object of the specified type
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="newRelationship"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CreateRelationship(repositoryId As String, newRelationship As Core.cmisObjectType,
                                  addACEs As Core.Security.cmisAccessControlListType,
                                  removeACEs As Core.Security.cmisAccessControlListType) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Deletes the content stream for the specified document object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="changeToken"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' A repository MAY automatically create new document versions as part of this service method. Therefore, the obejctId output NEED NOT be identical to the objectId input.
      ''' </remarks>
      Function DeleteContentStream(repositoryId As String, objectId As String, changeToken As String) As ccg.Result(Of Messaging.Responses.deleteContentStreamResponse)

      ''' <summary>
      ''' Returns True, if the submitted document was successfully removed
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="allVersions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function DeleteObject(repositoryId As String, objectId As String, allVersions As Boolean) As Exception

      ''' <summary>
      ''' Deletes the speciﬁed folder object and all of its child- and descendant-objects.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="allVersions">If TRUE (default), then delete all versions of all documents. If FALSE, delete only the document versions referenced in the tree. The repository MUST ignore the value of this parameter when this service is invoked on any non-document objects or non-versionable document objects.</param>
      ''' <param name="unfileObjects"></param>
      ''' <param name="continueOnFailure">If TRUE, then the repository SHOULD continue attempting to perform this operation even if deletion of a child- or descendant-object in the specified folder cannot be deleted. Default: False</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function DeleteTree(repositoryId As String, folderId As String, allVersions As Boolean, unfileObjects As Core.enumUnfileObject?, continueOnFailure As Boolean) As ccg.Result(Of Messaging.Responses.deleteTreeResponse)

      ''' <summary>
      ''' Returns the allowable actions for the specified document.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetAllowableActions(repositoryId As String, objectId As String) As ccg.Result(Of Core.cmisAllowableActionsType)

      ''' <summary>
      ''' Returns the content stream of the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="streamId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetContentStream(repositoryId As String, objectId As String, streamId As String) As ccg.Result(Of Messaging.cmisContentStreamType)

      ''' <summary>
      ''' Gets the specified information for the object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="includePolicyIds"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeACL"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="returnVersion"></param>
      ''' <param name="privateWorkingCopy">If True the private working copy of the document specified by objectId is requested</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetObject(repositoryId As String, objectId As String, filter As String, includeRelationships As Core.enumIncludeRelationships?,
                         includePolicyIds As Boolean?, renditionFilter As String, includeACL As Boolean?, includeAllowableActions As Boolean?,
                         returnVersion As RestAtom.enumReturnVersion?, privateWorkingCopy As Boolean?) As ccg.Result(Of cs.cmisObjectType)

      Function GetObjectByPath(repositoryId As String, path As String, filter As String, includeAllowableActions As Boolean?,
                               includePolicyIds As Boolean?, includeRelationships As Core.enumIncludeRelationships?,
                               includeACL As Boolean?, renditionFilter As String) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="targetFolderId"></param>
      ''' <param name="sourceFolderId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function MoveObject(repositoryId As String, objectId As String, targetFolderId As String, sourceFolderId As String) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Sets the content stream for the speciﬁed document object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="contentStream"></param>
      ''' <param name="overwriteFlag"></param>
      ''' <param name="changeToken"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function SetContentStream(repositoryId As String, objectId As String, contentStream As IO.Stream,
                                mimeType As String, fileName As String,
                                overwriteFlag As Boolean, changeToken As String) As ccg.Result(Of Messaging.Responses.setContentStreamResponse)

      ''' <summary>
      ''' Updates the submitted cmis-object
      ''' </summary>
      Function UpdateProperties(repositoryId As String, objectId As String, properties As cc.Collections.cmisPropertiesType, changeToken As String) As ccg.Result(Of cs.cmisObjectType)
#End Region

#Region "Multi"
      ''' <summary>
      ''' Adds an existing fileable non-folder object to a folder.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="folderId"></param>
      ''' <param name="allVersions">Add all versions of the object to the folder if the repository supports version-specific filing. Defaults to TRUE.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function AddObjectToFolder(repositoryId As String, objectId As String, folderId As String, allVersions As Boolean) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="folderId">The folder from which the object is to be removed.
      ''' If no value is specified, then the repository MUST remove the object from all folders in which it is currently filed.</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function RemoveObjectFromFolder(repositoryId As String, objectId As String, folderId As String) As ccg.Result(Of cs.cmisObjectType)
#End Region

#Region "Discovery"
      ''' <summary>
      ''' Returns a list of content changes
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="filter"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="includeACL"></param>
      ''' <param name="includePolicyIds"></param>
      ''' <param name="includeProperties"></param>
      ''' <param name="changeLogToken"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetContentChanges(repositoryId As String, filter As String, maxItems As xs_Integer?,
                                 includeACL As Boolean?, includePolicyIds As Boolean, includeProperties As Boolean,
                                 ByRef changeLogToken As String) As ccg.Result(Of cs.getContentChanges)

      ''' <summary>
      ''' Returns the data described by the specified CMIS query.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="q"></param>
      ''' <param name="searchAllVersions"></param>
      ''' <param name="includeRelationships"></param>
      ''' <param name="renditionFilter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="skipCount"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function Query(repositoryId As String, q As String, searchAllVersions As Boolean, includeRelationships As Core.enumIncludeRelationships?,
                     renditionFilter As String, includeAllowableActions As Boolean, maxItems As xs_Integer?, skipCount As xs_Integer?) As ccg.Result(Of cs.cmisObjectListType)
#End Region

#Region "Versioning"
      ''' <summary>
      ''' Rollback a CheckOut-action
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId">Id of a private working copy object</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CancelCheckOut(repositoryId As String, objectId As String) As Exception

      ''' <summary>
      ''' Checks-in the Private Working Copy document.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="major">TRUE (default) if the checked-in document object MUST be a major version.</param>
      ''' <param name="checkInComment"></param>
      ''' <param name="addACEs">A list of ACEs that MUST be added to the newly-created document object.</param>
      ''' <param name="removeACEs">A list of ACEs that MUST be removed from the newly-created document object.</param>
      ''' <returns></returns>
      ''' <remarks>
      ''' For repositories that do NOT support the optional capabilityPWCUpdatable capability, the properties and contentStream input parameters MUST be
      ''' provided on the checkIn service for updates to happen as part of checkIn.
      ''' Each CMIS protocol binding MUST specify whether the checkin service MUST always include all updatable properties, or only those properties
      ''' whose values are different than the original value of the object.
      ''' </remarks>
      Function CheckIn(repositoryId As String, objectId As String,
                       properties As Core.Collections.cmisPropertiesType, policies As String(),
                       content As Messaging.cmisContentStreamType,
                       major As Boolean, checkInComment As String,
                       Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                       Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Checks out the specified CMIS object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function CheckOut(repositoryId As String, objectId As String) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Returns all Documents in the specified version series.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="versionSeriesId"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetAllVersions(repositoryId As String, objectId As String, versionSeriesId As String, filter As String, includeAllowableActions As Boolean?) As ccg.Result(Of cs.cmisObjectListType)
#End Region

#Region "Relationships"
      ''' <summary>
      ''' Returns the relationships for the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="includeSubRelationshipTypes"></param>
      ''' <param name="relationshipDirection"></param>
      ''' <param name="typeId"></param>
      ''' <param name="maxItems"></param>
      ''' <param name="skipCount"></param>
      ''' <param name="filter"></param>
      ''' <param name="includeAllowableActions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetObjectRelationships(repositoryId As String, objectId As String, includeSubRelationshipTypes As Boolean,
                                      relationshipDirection As Core.enumRelationshipDirection?,
                                      typeId As String, maxItems As xs_Integer?, skipCount As xs_Integer?,
                                      filter As String, includeAllowableActions As Boolean?) As ccg.Result(Of cs.cmisObjectListType)
#End Region

#Region "Policy"
      ''' <summary>
      ''' Applies a policy to the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="policyId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function ApplyPolicy(repositoryId As String, objectId As String, policyId As String) As ccg.Result(Of cs.cmisObjectType)

      ''' <summary>
      ''' Returns a list of policies applied to the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="filter"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetAppliedPolicies(repositoryId As String, objectId As String, filter As String) As ccg.Result(Of cs.cmisObjectListType)

      ''' <summary>
      ''' Removes a policy from the specified object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="policyId"></param>
      ''' <remarks></remarks>
      Function RemovePolicy(repositoryId As String, objectId As String, policyId As String) As Exception
#End Region

#Region "ACL"
      ''' <summary>
      ''' Adds or removes the given ACEs to or from the ACL of document or folder object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="addACEs"></param>
      ''' <param name="removeACEs"></param>
      ''' <param name="aclPropagation"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function ApplyACL(repositoryId As String, objectId As String,
                        addACEs As Core.Security.cmisAccessControlListType, removeACEs As Core.Security.cmisAccessControlListType,
                        aclPropagation As Core.enumACLPropagation) As ccg.Result(Of Core.Security.cmisAccessControlListType)

      ''' <summary>
      ''' Get the ACL currently applied to the specified document or folder object.
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="objectId"></param>
      ''' <param name="onlyBasicPermissions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetACL(repositoryId As String, objectId As String, onlyBasicPermissions As Boolean) As ccg.Result(Of Core.Security.cmisAccessControlListType)
#End Region

      ''' <summary>
      ''' Returns the baseUri of the service
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      ReadOnly Property BaseUri As Uri

      ''' <summary>
      ''' Returns True if the objectId exists in the repository
      ''' </summary>
      ''' <param name="objectId"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      ReadOnly Property Exists(repositoryId As String, objectId As String) As Boolean

      ''' <summary>
      ''' Returns the BaseObjectType of cmisObject specified by objectId
      ''' </summary>
      Function GetBaseObjectType(repositoryId As String, objectId As String) As Core.enumBaseObjectTypeIds

      ''' <summary>
      ''' Returns the objectId of the object specified by path.
      ''' </summary>
      ''' <param name="path"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetObjectId(repositoryId As String, path As String) As String

      ''' <summary>
      ''' Returns the parent-typeId of the specified type. If the specified type is a
      ''' base type, the function returns null.
      ''' </summary>
      ''' <param name="typeId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetParentTypeId(repositoryId As String, typeId As String) As String

      ''' <summary>
      ''' Returns the cookie for the sessionId
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetSessionIdCookieName() As String

      ''' <summary>
      ''' Returns the author for lists of types or objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function GetSystemAuthor() As sss.SyndicationPerson

      ''' <summary>
      ''' Log exception called before the cmisService throws an exception
      ''' </summary>
      ''' <param name="ex"></param>
      ''' <remarks>Compiler constant EnableExceptionLogging must be set to 'True'</remarks>
      Sub LogException(ex As Exception)

      ''' <summary>
      ''' Returns the cmisRepositoryInfoType-object for specified repositoryId
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      ReadOnly Property RepositoryInfo(repositoryId As String) As Core.cmisRepositoryInfoType

      ''' <summary>
      ''' Returns the cmisTypeDefinitionType-object for the specified typeId
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      ReadOnly Property TypeDefinition(repositoryId As String, typeId As String) As Core.Definitions.Types.cmisTypeDefinitionType

      ''' <summary>
      ''' Returns True if userName describes a known user and the password is valid
      ''' </summary>
      ''' <param name="userName"></param>
      ''' <param name="password"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Function ValidateUserNamePassword(userName As String, password As String) As Boolean

   End Interface
End Namespace