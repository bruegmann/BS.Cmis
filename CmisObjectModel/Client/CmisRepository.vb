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
Imports cmr = CmisObjectModel.Messaging.Requests
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports ssc = System.Security.Cryptography
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Client
   ''' <summary>
   ''' Simplifies requests to a cmis repository
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisRepository
      Inherits CmisDataModelObject

#Region "Constructors"
      Public Sub New(repositoryInfo As Core.cmisRepositoryInfoType, client As Contracts.ICmisClient)
         MyBase.New(client, repositoryInfo)
      End Sub
#End Region

#Region "Repository"
      ''' <summary>
      ''' Creates a new type definition that is a subtype of an existing specified parent type
      ''' </summary>
      ''' <remarks></remarks>
      Public Function CreateType(type As Core.Definitions.Types.cmisTypeDefinitionType) As CmisType
         With _client.CreateType(New cmr.createType() With {.RepositoryId = _repositoryInfo.RepositoryId, .Type = type})
            _lastException = .Exception
            Return If(_lastException Is Nothing, CreateCmisType(.Response.Type), Nothing)
         End With
      End Function

      ''' <summary>
      ''' Returns the list of object-base-types defined for the repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetBaseTypes(Optional includePropertyDefinitions As Boolean = False,
                                   Optional maxItems As xs_Integer? = Nothing,
                                   Optional skipCount As xs_Integer? = Nothing) As Generic.ItemList(Of CmisType)
         Return MyBase.GetTypeChildren(Nothing, includePropertyDefinitions, maxItems, skipCount)
      End Function

      ''' <summary>
      ''' Gets the definition of the specified object-type
      ''' </summary>
      ''' <remarks></remarks>
      Public Shadows Function GetTypeDefinition(typeId As String) As CmisType
         Return MyBase.GetTypeDefinition(typeId)
      End Function

      ''' <summary>
      ''' Returns the set of the descendant object-types defined for the Repository under the specified type
      ''' </summary>
      ''' <remarks></remarks>
      Public Shadows Function GetTypeDescendants(Optional typeId As String = Nothing,
                                                 Optional depth As xs_Integer? = Nothing,
                                                 Optional includePropertyDefinitions As Boolean = False) As Generic.ItemContainer(Of CmisType)()
         Return MyBase.GetTypeDescendants(typeId, depth, includePropertyDefinitions)
      End Function

      ''' <summary>
      ''' Returns the set of the object-types defined for the Repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetTypes(Optional includePropertyDefinitions As Boolean = False) As Generic.ItemContainer(Of CmisType)()
         Return MyBase.GetTypeDescendants(Nothing, Nothing, includePropertyDefinitions)
      End Function
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Gets the list of documents that are checked out that the user has access to
      ''' </summary>
      ''' <remarks></remarks>
      Public Shadows Function GetCheckedOutDocs(Optional maxItems As xs_Integer? = Nothing,
                                                Optional skipCount As xs_Integer? = Nothing,
                                                Optional orderBy As String = Nothing,
                                                Optional filter As String = Nothing,
                                                Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                Optional renditionFilter As String = Nothing,
                                                Optional includeAllowableActions As Boolean? = Nothing) As Generic.ItemList(Of CmisObject)
         Return MyBase.GetCheckedOutDocs(Nothing, maxItems, skipCount, orderBy, filter, includeRelationships, renditionFilter, includeAllowableActions)
      End Function
#End Region

#Region "Object"
      ''' <summary>
      ''' Updates properties and secondary types of one or more objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>Notice: using the AtomPub-Binding new object ids are not exposed</remarks>
      Public Function BulkUpdateProperties(objectIdAndChangeTokens As Core.cmisObjectIdAndChangeTokenType(),
                                           Optional properties As Core.Collections.cmisPropertiesType = Nothing,
                                           Optional addSecondaryTypeIds As String() = Nothing,
                                           Optional removeSecondaryTypeIds As String() = Nothing) As Core.cmisObjectIdAndChangeTokenType()
         Dim bulkUpdateData As New Core.cmisBulkUpdateType() With {.ObjectIdAndChangeTokens = objectIdAndChangeTokens,
                                                                   .Properties = properties,
                                                                   .AddSecondaryTypeIds = addSecondaryTypeIds,
                                                                   .RemoveSecondaryTypeIds = removeSecondaryTypeIds}
         With _client.BulkUpdateProperties(New cmr.bulkUpdateProperties() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                                .BulkUpdateData = bulkUpdateData})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return .Response.ObjectIdAndChangeTokens
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a document object of the specified type (given by the cmis:objectTypeId property) in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function CreateDocument(properties As Core.Collections.cmisPropertiesType,
                                             Optional folderId As String = Nothing,
                                             Optional contentStream As Messaging.cmisContentStreamType = Nothing,
                                             Optional versioningState As Core.enumVersioningState? = Nothing,
                                             Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                             Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                             Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisDocument
         Return MyBase.CreateDocument(properties, folderId, contentStream, versioningState, policies, addACEs, removeACEs)
      End Function

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the (optionally) specified location
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function CreateDocumentFromSource(sourceId As String,
                                                       Optional properties As Core.Collections.cmisPropertiesType = Nothing,
                                                       Optional folderId As String = Nothing,
                                                       Optional versioningState As Core.enumVersioningState? = Nothing,
                                                       Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                                       Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                                       Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisDocument
         Return MyBase.CreateDocumentFromSource(sourceId, properties, folderId, versioningState, policies, addACEs, removeACEs)
      End Function

      ''' <summary>
      ''' Creates a folder object of the specified type (given by the cmis:objectTypeId property)
      ''' </summary>
      ''' <remarks></remarks>
      Public Function CreateFolder(properties As Core.Collections.cmisPropertiesType,
                                   Optional folderId As String = Nothing,
                                   Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                   Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                   Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisFolder
         With _client.CreateFolder(New cmr.createFolder() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                .FolderId = If(String.IsNullOrEmpty(folderId), RootFolderId, folderId),
                                                                .Properties = properties, .Policies = policies, .AddACEs = addACEs, .RemoveACEs = removeACEs})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return TryCast(GetObject(.Response.ObjectId), CmisFolder)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a relationship object of the specified type (given by the cmis:objectTypeId property)
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateRelationship(properties As Core.Collections.cmisPropertiesType,
                                         Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                         Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                         Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisRelationship
         With _client.CreateRelationship(New cmr.createRelationship() With {.RepositoryId = _repositoryInfo.RepositoryId, .Properties = properties,
                                                                            .Policies = policies, .AddACEs = addACEs, .RemoveACEs = removeACEs})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return TryCast(GetObject(.Response.ObjectId), CmisRelationship)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function GetObject(objectId As String,
                                        Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                        Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                        Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing) As CmisObject
         Return MyBase.GetObject(objectId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions)
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function GetObject(Of TResult As CmisObject)(objectId As String,
                                                                  Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                                  Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                                                  Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing) As TResult
         Return TryCast(MyBase.GetObject(objectId, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions), TResult)
      End Function

      ''' <summary>
      ''' Gets the specified information for the latest object in the version series
      ''' </summary>
      Public Shadows Function GetObjectOfLatestVersion(objectId As String,
                                                       Optional major As Boolean = False, Optional filter As String = Nothing,
                                                       Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                       Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                                       Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing,
                                                       Optional acceptPWC As Common.enumCheckedOutState = enumCheckedOutState.notCheckedOut) As CmisDocument
         Return MyBase.GetObjectOfLatestVersion(objectId, major, filter, includeRelationships, includePolicyIds,
                                                renditionFilter, includeACL, includeAllowableActions, acceptPWC)
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetObjectByPath(path As String,
                                      Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                      Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                      Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing) As CmisObject
         With _client.GetObjectByPath(New cmr.getObjectByPath() With {.RepositoryId = _repositoryInfo.RepositoryId, .Path = path,
                                                                      .Filter = filter, .IncludeRelationships = includeRelationships,
                                                                      .IncludePolicyIds = includePolicyIds, .RenditionFilter = renditionFilter,
                                                                      .IncludeACL = includeACL, .IncludeAllowableActions = includeAllowableActions})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return CreateCmisObject(.Response.Object)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the specified information for the object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetObjectByPath(Of TResult As CmisObject)(path As String,
                                                                Optional filter As String = Nothing, Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                                Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                                                Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing) As TResult
         Return TryCast(Me.GetObjectByPath(path, filter, includeRelationships, includePolicyIds, renditionFilter, includeACL, includeAllowableActions), TResult)
      End Function

      ''' <summary>
      ''' Moves the specified file-able object from one folder to another
      ''' </summary>
      ''' <remarks></remarks>
      Public Shadows Function MoveObject(objectId As String, targetFolderId As String, sourceFolderId As String) As CmisObject
         Return MyBase.MoveObject(objectId, targetFolderId, sourceFolderId)
      End Function
#End Region

#Region "Discovery"
      ''' <summary>
      ''' Gets a list of content changes. This service is intended to be used by search crawlers or other applications that need to
      ''' efficiently understand what has changed in the repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>Caution: Through the AtomPub binding it is not possible to retrieve the ChangeLog Token</remarks>
      Public Function GetContentChanges(ByRef changeLogToken As String,
                                        Optional includeProperties As Boolean = False,
                                        Optional includePolicyIds As Boolean = False,
                                        Optional includeACL As Boolean? = Nothing,
                                        Optional maxItems As xs_Integer? = Nothing) As Generic.ItemList(Of CmisObject)
         With _client.GetContentChanges(New cmr.getContentChanges() With {.RepositoryId = _repositoryInfo.RepositoryId, .ChangeLogToken = changeLogToken,
                                                                          .IncludeProperties = includeProperties, .IncludePolicyIds = includePolicyIds,
                                                                          .IncludeACL = includeACL, .MaxItems = maxItems})
            _lastException = .Exception
            If _lastException Is Nothing Then
               changeLogToken = .Response.ChangeLogToken
               Return Convert(.Response.Objects)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Executes a CMIS query statement against the contents of the repository
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Query(statement As String,
                            Optional searchAllVersions As Boolean? = Nothing,
                            Optional includeRelationships As Core.enumIncludeRelationships = Core.enumIncludeRelationships.none,
                            Optional renditionFilter As String = Nothing,
                            Optional includeAllowableActions As Boolean = False,
                            Optional maxItems As xs_Integer? = Nothing,
                            Optional skipCount As xs_Integer? = Nothing) As Generic.ItemList(Of CmisObject)
         With _client.Query(New cmr.query() With {.RepositoryId = _repositoryInfo.RepositoryId, .Statement = statement, .SearchAllVersions = searchAllVersions,
                                                  .IncludeRelationships = includeRelationships, .RenditionFilter = renditionFilter,
                                                  .IncludeAllowableActions = includeAllowableActions, .MaxItems = maxItems, .SkipCount = skipCount})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return Convert(.Response.Objects)
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "Versioning"
      ''' <summary>
      ''' Reverses the effect of a check-out (checkOut). Removes the Private Working Copy of the checked-out document, allowing other documents
      ''' in the version series to be checked out again. If the private working copy has been created by createDocument, cancelCheckOut MUST
      ''' delete the created document
      ''' </summary>
      Public Function CancelCheckOut(objectId As String,
                                     Optional major As Boolean = False,
                                     Optional filter As String = Nothing,
                                     Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                     Optional includePolicyIds As Boolean? = Nothing,
                                     Optional renditionFilter As String = Nothing,
                                     Optional includeACL As Boolean? = Nothing,
                                     Optional includeAllowableActions As Boolean? = Nothing) As CmisDocument
         Dim document As CmisDocument = GetObject(Of CmisDocument)(objectId, String.Join(",", CmisPredefinedPropertyNames.VersionSeriesId,
                                                                                              CmisPredefinedPropertyNames.IsPrivateWorkingCopy))

         'GetObject mit Filterangabe führt bei AGORUM zu keinem Ergebnis. Deshalb hier nochmal ohne Filter anfragen, wenn document Nothing ist. Siehe #7522.
         If document Is Nothing Then
            document = GetObject(Of CmisDocument)(objectId)
         End If

         Dim isPrivateWorkingCopy As Boolean? = If(document Is Nothing, Nothing, document.IsPrivateWorkingCopy)

         With _client.CancelCheckOut(New cmr.cancelCheckOut() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                    .ObjectId = objectId,
                                                                    .PWCLinkRequired = Not (isPrivateWorkingCopy.HasValue AndAlso isPrivateWorkingCopy.Value)})
            _lastException = .Exception
            Return If(_lastException Is Nothing, GetObjectOfLatestVersion(If(document.VersionSeriesId.Value, objectId), major, filter,
                                                                          includeRelationships, includePolicyIds, renditionFilter,
                                                                          includeACL, includeAllowableActions, enumCheckedOutState.notCheckedOut), Nothing)
         End With
      End Function

      ''' <summary>
      ''' Checks-in the Private Working Copy document
      ''' </summary>
      Public Function CheckIn(objectId As String,
                              Optional properties As CmisObjectModel.Core.Collections.cmisPropertiesType = Nothing,
                              Optional contentStream As CmisObjectModel.Messaging.cmisContentStreamType = Nothing,
                              Optional checkinComment As String = Nothing,
                              Optional major As Boolean = True,
                              Optional filter As String = Nothing,
                              Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                              Optional includePolicyIds As Boolean? = Nothing,
                              Optional renditionFilter As String = Nothing,
                              Optional includeACL As Boolean? = Nothing,
                              Optional includeAllowableActions As Boolean? = Nothing) As CmisDocument
         With _client.CheckIn(New cmr.checkIn() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                     .ObjectId = objectId,
                                                     .Major = major, .Properties = properties, .ContentStream = contentStream,
                                                     .CheckinComment = checkinComment})
            _lastException = .Exception
            Return If(_lastException Is Nothing, TryCast(GetObject(.Response.ObjectId, filter,
                                                                   includeRelationships, includePolicyIds,
                                                                   renditionFilter, includeACL,
                                                                   includeAllowableActions), CmisDocument), Nothing)
         End With
      End Function

      ''' <summary>
      ''' Create a private working copy (PWC) of the document
      ''' </summary>
      Public Function CheckOut(objectId As String,
                               Optional filter As String = Nothing,
                               Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                               Optional includePolicyIds As Boolean? = Nothing,
                               Optional renditionFilter As String = Nothing,
                               Optional includeACL As Boolean? = Nothing,
                               Optional includeAllowableActions As Boolean? = Nothing) As CmisDocument
         With _client.CheckOut(New cmr.checkOut() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = objectId})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Dim retVal As CmisDocument = TryCast(GetObject(.Response.ObjectId, filter,
                                                              includeRelationships, includePolicyIds,
                                                              renditionFilter, includeACL,
                                                              includeAllowableActions), CmisDocument)
               If retVal IsNot Nothing Then retVal.CancelCheckOutFallbackId = objectId
               Return retVal
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "pass-through properties"
      Public ReadOnly Property AclCapabilities As Core.Security.cmisACLCapabilityType
         Get
            Return _repositoryInfo.AclCapability
         End Get
      End Property

      Public ReadOnly Property Capabilities As Core.cmisRepositoryCapabilitiesType
         Get
            Return _repositoryInfo.Capabilities
         End Get
      End Property

      Public ReadOnly Property ChangesIncomplete As Boolean?
         Get
            Return _repositoryInfo.ChangesIncomplete
         End Get
      End Property

      Public ReadOnly Property ChangesOnTypes As Core.enumBaseObjectTypeIds()
         Get
            Return _repositoryInfo.ChangesOnTypes
         End Get
      End Property

      Public ReadOnly Property CmisVersionSupported As String
         Get
            Return _repositoryInfo.CmisVersionSupported
         End Get
      End Property

      Public ReadOnly Property Description As String
         Get
            Return _repositoryInfo.RepositoryDescription
         End Get
      End Property

      Public ReadOnly Property Id As String
         Get
            Return _repositoryInfo.RepositoryId
         End Get
      End Property

      Public ReadOnly Property LatestChangeLogToken As String
         Get
            Return _repositoryInfo.LatestChangeLogToken
         End Get
      End Property

      Public ReadOnly Property Name As String
         Get
            Return _repositoryInfo.RepositoryName
         End Get
      End Property

      Public ReadOnly Property PrincipalAnonymus As String
         Get
            Return _repositoryInfo.PrincipalAnonymous
         End Get
      End Property

      Public ReadOnly Property PrincipalAnyone As String
         Get
            Return _repositoryInfo.PrincipalAnyone
         End Get
      End Property

      Public ReadOnly Property ProductName As String
         Get
            Return _repositoryInfo.ProductName
         End Get
      End Property

      Public ReadOnly Property ProductVersion As String
         Get
            Return _repositoryInfo.ProductVersion
         End Get
      End Property

      Public ReadOnly Property RootFolderId As String
         Get
            Return _repositoryInfo.RootFolderId
         End Get
      End Property

      Public ReadOnly Property ThinClientUri As String
         Get
            Return _repositoryInfo.ThinClientURI
         End Get
      End Property

      Public ReadOnly Property VendorName As String
         Get
            Return _repositoryInfo.VendorName
         End Get
      End Property
#End Region

      ''' <summary>
      ''' Returns the root folder object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetRootFolder(Optional filter As String = Nothing,
                                    Optional includePolicyIds As Boolean? = Nothing, Optional renditionFilter As String = Nothing,
                                    Optional includeACL As Boolean? = Nothing, Optional includeAllowableActions As Boolean? = Nothing) As CmisFolder
         Return TryCast(GetObject(_repositoryInfo.RootFolderId, filter, Nothing, includePolicyIds, renditionFilter, includeACL, includeAllowableActions), CmisFolder)
      End Function

      ''' <summary>
      ''' Returns true if the repository supports holds
      ''' </summary>
      Public Shadows ReadOnly Property HoldCapability As Boolean
         Get
            Return MyBase.HoldCapability
         End Get
      End Property

      ''' <summary>
      ''' Logs out from repository
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub Logout()
         _client.Logout(_repositoryInfo.RepositoryId)
      End Sub

      ''' <summary>
      ''' Tells the server, that this client is still alive
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub Ping()
         _client.Ping(_repositoryInfo.RepositoryId)
      End Sub

      ''' <summary>
      ''' Returns the retentions supported by the repository
      ''' </summary>
      Public Shadows ReadOnly Property RetentionCapability As enumRetentionCapability
         Get
            Return MyBase.RetentionCapability
         End Get
      End Property

   End Class
End Namespace