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
Imports ccg = CmisObjectModel.Common.Generic
Imports cmr = CmisObjectModel.Messaging.Requests
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports ssc = System.Security.Cryptography
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#If Not xs_HttpRequestAddRange64 Then
#Const HttpRequestAddRangeShortened = True
#End If
#End If

Namespace CmisObjectModel.Client
   ''' <summary>
   ''' Simplifies requests to cmis document, cmis folder, cmis policy, cmis relationship
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisObject
      Inherits CmisDataModelObject

#Region "Constructors"
      Public Sub New(cmisObject As Core.cmisObjectType,
                     client As Contracts.ICmisClient, repositoryInfo As Core.cmisRepositoryInfoType)
         MyBase.New(client, repositoryInfo)
         _cmisObject = cmisObject
      End Sub
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Creates the CmisType-Instance
      ''' </summary>
      ''' <remarks></remarks>
      Public Class PreStage
         Public Sub New(client As Contracts.ICmisClient, cmisObject As Core.cmisObjectType)
            _client = client
            _cmisObject = cmisObject
         End Sub

         Private _client As Contracts.ICmisClient
         Private _cmisObject As Core.cmisObjectType

         Public Shared Operator +(arg1 As PreStage, arg2 As Core.cmisRepositoryInfoType) As CmisObject
            Dim baseTypeId As ccg.Nullable(Of String) = If(arg1._cmisObject Is Nothing, Nothing, arg1._cmisObject.BaseTypeId)

            Select Case If(baseTypeId.HasValue, baseTypeId.Value, String.Empty)
               Case Core.enumBaseObjectTypeIds.cmisDocument.GetName()
                  Return New Client.CmisDocument(arg1._cmisObject, arg1._client, arg2)
               Case Core.enumBaseObjectTypeIds.cmisFolder.GetName()
                  Return New Client.CmisFolder(arg1._cmisObject, arg1._client, arg2)
               Case Core.enumBaseObjectTypeIds.cmisPolicy.GetName()
                  Return New Client.CmisPolicy(arg1._cmisObject, arg1._client, arg2)
               Case Core.enumBaseObjectTypeIds.cmisRelationship.GetName()
                  Return New Client.CmisRelationship(arg1._cmisObject, arg1._client, arg2)
               Case Else
                  Return New Client.CmisObject(arg1._cmisObject, arg1._client, arg2)
            End Select
         End Operator
      End Class
#End Region

#Region "Predefined properties"
      Public Overridable Property BaseTypeId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.BaseTypeId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.BaseTypeId = value
         End Set
      End Property

      Public Overridable Property ChangeToken As ccg.Nullable(Of String)
         Get
            Return _cmisObject.ChangeToken
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.ChangeToken = value
         End Set
      End Property

      Public Overridable Property CreatedBy As ccg.Nullable(Of String)
         Get
            Return _cmisObject.CreatedBy
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.CreatedBy = value
         End Set
      End Property

      Public Overridable Property CreationDate As DateTimeOffset?
         Get
            Return _cmisObject.CreationDate
         End Get
         Set(value As DateTimeOffset?)
            _cmisObject.CreationDate = value
         End Set
      End Property

      Public Overridable Property Description As ccg.Nullable(Of String)
         Get
            Return _cmisObject.Description
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.Description = value
         End Set
      End Property

      Public Overridable Property LastModificationDate As DateTimeOffset?
         Get
            Return _cmisObject.LastModificationDate
         End Get
         Set(value As DateTimeOffset?)
            _cmisObject.LastModificationDate = value
         End Set
      End Property

      Public Overridable Property LastModifiedBy As ccg.Nullable(Of String)
         Get
            Return _cmisObject.LastModifiedBy
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.LastModifiedBy = value
         End Set
      End Property

      Public Overridable Property Name As ccg.Nullable(Of String)
         Get
            Return _cmisObject.Name
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.Name = value
         End Set
      End Property

      Public Overridable Property ObjectId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.ObjectId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.ObjectId = value
         End Set
      End Property

      Public Overridable Property ObjectTypeId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.ObjectTypeId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.ObjectTypeId = value
         End Set
      End Property

      Public Overridable Property SecondaryObjectTypeIds As ccg.Nullable(Of String())
         Get
            Return _cmisObject.SecondaryObjectTypeIds
         End Get
         Set(value As ccg.Nullable(Of String()))
            _cmisObject.SecondaryObjectTypeIds = value
         End Set
      End Property
#End Region

#Region "Pass-through-methods"
      ''' <summary>
      ''' Returns the objectTypeId followed by the secondaryObjectTypeIds separated by comma
      ''' </summary>
      Public Overridable Function GetCompositeObjectTypeId() As String
         Return _cmisObject.GetCompositeObjectTypeId()
      End Function

      ''' <summary>
      ''' Returns as first element the objectTypeId of the current object followed by the secondaryTypeIds if defined
      ''' </summary>
      Public Overridable Function GetObjectTypeIds() As IEnumerable(Of String)
         Return _cmisObject.GetObjectTypeIds()
      End Function
#End Region

#Region "Pass-through-properties"
      Public Overridable Property Acl As Core.Security.cmisAccessControlListType
         Get
            Return _cmisObject.Acl
         End Get
         Set(value As Core.Security.cmisAccessControlListType)
            _cmisObject.Acl = value
         End Set
      End Property

      Public Overridable Property AllowableActions As Core.cmisAllowableActionsType
         Get
            Return _cmisObject.AllowableActions
         End Get
         Set(value As Core.cmisAllowableActionsType)
            _cmisObject.AllowableActions = value
         End Set
      End Property

      Public Overridable Property ChangeEventInfo As Core.cmisChangeEventType
         Get
            Return _cmisObject.ChangeEventInfo
         End Get
         Set(value As Core.cmisChangeEventType)
            _cmisObject.ChangeEventInfo = value
         End Set
      End Property

      Public Overridable Property ExactAcl As Boolean?
         Get
            Return _cmisObject.ExactACL
         End Get
         Set(value As Boolean?)
            _cmisObject.ExactACL = value
         End Set
      End Property

      Public Overridable Property PolicyIds As Core.Collections.cmisListOfIdsType
         Get
            Return _cmisObject.PolicyIds
         End Get
         Set(value As Core.Collections.cmisListOfIdsType)
            _cmisObject.PolicyIds = value
         End Set
      End Property

      Public Overridable Property Properties As Core.Collections.cmisPropertiesType
         Get
            Return _cmisObject.Properties
         End Get
         Set(value As Core.Collections.cmisPropertiesType)
            _cmisObject.Properties = value
         End Set
      End Property

      Public Overridable Property Relationships As CmisRelationship()
         Get
            If _cmisObject.Relationships Is Nothing Then
               Return Nothing
            Else
               Return (From relationship As Core.cmisObjectType In _cmisObject.Relationships
                       Select New CmisRelationship(relationship, _client, _repositoryInfo)).ToArray()
            End If
         End Get
         Set(value As CmisRelationship())
            If value Is Nothing Then
               _cmisObject.Relationships = Nothing
            Else
               _cmisObject.Relationships = (From relationship As CmisRelationship In value
                                            Select If(relationship Is Nothing, Nothing, relationship._cmisObject)).ToArray()
            End If
         End Set
      End Property

      Public Overridable Property Renditions As Core.cmisRenditionType()
         Get
            Return _cmisObject.Renditions
         End Get
         Set(value As Core.cmisRenditionType())
            _cmisObject.Renditions = value
         End Set
      End Property
#End Region

#Region "Repository"
      ''' <summary>
      ''' Returns BaseType for the current object or null, if BaseTypeId is not specified
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetObjectBaseType() As CmisType
         Return GetTypeDefinition(Me.BaseTypeId)
      End Function

      ''' <summary>
      ''' Returns ObjectType for the current object or null, if ObjectTypeId is not specified
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetObjectType() As CmisType
         Return GetTypeDefinition(Me.ObjectTypeId)
      End Function
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Gets the parent folder(s) for the current fileable object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetParents(Optional filter As String = Nothing,
                                 Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                 Optional renditionFilter As String = Nothing,
                                 Optional includeAllowableActions As Boolean? = Nothing,
                                 Optional includeRelativePathSegment As Boolean? = Nothing) As CmisObject()
         With _client.GetObjectParents(New cmr.getObjectParents() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                        .Filter = filter, .IncludeRelationships = includeRelationships,
                                                                        .RenditionFilter = renditionFilter, .IncludeAllowableActions = includeAllowableActions,
                                                                        .IncludeRelativePathSegment = includeRelativePathSegment})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Dim result As New List(Of CmisObject)

               If .Response.Parents IsNot Nothing Then
                  For Each parent As Messaging.cmisObjectParentsType In .Response.Parents
                     Dim cmisObject As CmisObject = CreateCmisObject(parent.Object)

                     cmisObject.RelativePathSegment = parent.RelativePathSegment
                     result.Add(cmisObject)
                  Next
               End If

               Return result.ToArray()
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "Object"
      ''' <summary>
      ''' Deletes the current object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function DeleteObject(Optional allVersions As Boolean = True) As Boolean
         With _client.DeleteObject(New cmr.deleteObject() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId, .AllVersions = allVersions})
            _lastException = .Exception
            Return _lastException Is Nothing
         End With
      End Function

      ''' <summary>
      ''' Gets the list of allowable actions for an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetAllowableActions() As Core.cmisAllowableActionsType
         With _client.GetAllowableActions(New cmr.getAllowableActions() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId})
            _lastException = .Exception
            Return If(_lastException Is Nothing, .Response.AllowableActions, Nothing)
         End With
      End Function

      ''' <summary>
      ''' Gets the content stream for the specified document object, or gets a rendition stream for a specified rendition of a document or folder object.
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
#If HttpRequestAddRangeShortened Then
      Protected Function GetContentStream(Optional streamId As String = Nothing,
                                          Optional offset As Integer? = Nothing,
                                          Optional length As Integer? = Nothing) As Messaging.cmisContentStreamType
#Else
      Protected Function GetContentStream(Optional streamId As String = Nothing,
                                          Optional offset As xs_Integer? = Nothing,
                                          Optional length As xs_Integer? = Nothing) As Messaging.cmisContentStreamType
#End If
         With _client.GetContentStream(New cmr.getContentStream() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                        .StreamId = streamId, .Offset = offset, .Length = length})
            _lastException = .Exception
            Return If(_lastException Is Nothing, .Response.ContentStream, Nothing)
         End With
      End Function

      ''' <summary>
      ''' Gets the list of properties for the current object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetProperties(Optional filter As String = Nothing) As Core.Collections.cmisPropertiesType
         With _client.GetProperties(New cmr.getProperties() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId, .Filter = filter})
            _lastException = .Exception
            Return If(_lastException Is Nothing, .Response.Properties, Nothing)
         End With
      End Function

      ''' <summary>
      ''' Gets the list of associated renditions for the specified object. Only rendition attributes are returned, not rendition stream
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetRenditions(Optional renditionFilter As String = Nothing,
                                    Optional maxItems As xs_Integer? = Nothing,
                                    Optional skipCount As xs_Integer? = Nothing) As Core.cmisRenditionType()
         With _client.GetRenditions(New cmr.getRenditions() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                  .RenditionFilter = renditionFilter, .MaxItems = maxItems, .SkipCount = skipCount})
            _lastException = .Exception
            Return If(_lastException Is Nothing, .Response.Renditions, Nothing)
         End With
      End Function

      ''' <summary>
      ''' Moves the current file-able object from one folder to another
      ''' </summary>
      ''' <remarks></remarks>
      Public Shadows Function Move(targetFolderId As String, sourceFolderId As String) As CmisObject
         Return MyBase.MoveObject(_cmisObject.ObjectId, targetFolderId, sourceFolderId)
      End Function

      ''' <summary>
      ''' Updates properties and secondary types of the current object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function UpdateProperties(properties As Core.Collections.cmisPropertiesType) As Boolean
         With _client.UpdateProperties(New cmr.updateProperties() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                        .Properties = properties, .ChangeToken = _cmisObject.ChangeToken})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Dim objectId As String = .Response.ObjectId
               Dim changeToken As String = .Response.ChangeToken

               If Not String.IsNullOrEmpty(objectId) Then _cmisObject.ObjectId = objectId
               If Not String.IsNullOrEmpty(changeToken) Then _cmisObject.ChangeToken = changeToken
               'update property values
               If Not (properties Is Nothing OrElse properties.Properties Is Nothing) Then
                  If _cmisObject.Properties Is Nothing Then
                     _cmisObject.Properties = properties
                  Else
                     Dim currentProperties As Dictionary(Of String, Core.Properties.cmisProperty) = _cmisObject.GetProperties(True)

                     For Each [property] As Core.Properties.cmisProperty In properties.Properties
                        Dim propertyDefinitionId As String = If([property].PropertyDefinitionId, "").ToLowerInvariant()
                        If currentProperties.ContainsKey(propertyDefinitionId) Then
                           currentProperties(propertyDefinitionId).Values = [property].Values
                        Else
                           _cmisObject.Properties.Append([property])
                        End If
                     Next
                  End If
               End If
               Return True
            Else
               Return False
            End If
         End With
      End Function
#End Region

#Region "Multi"
      ''' <summary>
      ''' Adds the current fileable non-folder object to a folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function AddObjectToFolder(folderId As String, Optional allVersions As Boolean = True) As Boolean
         With _client.AddObjectToFolder(New cmr.addObjectToFolder() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                          .FolderId = folderId, .AllVersions = allVersions})
            _lastException = .Exception
            Return _lastException Is Nothing
         End With
      End Function

      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function RemoveObjectFromFolder(Optional folderId As String = Nothing) As Boolean
         With _client.RemoveObjectFromFolder(New cmr.removeObjectFromFolder() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                                    .ObjectId = _cmisObject.ObjectId, .FolderId = folderId})
            _lastException = .Exception
            Return _lastException Is Nothing
         End With
      End Function
#End Region

#Region "Relationship"
      ''' <summary>
      ''' Gets all or a subset of relationships associated with an independent object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetObjectRelationships(Optional includeSubRelationshipTypes As Boolean = False,
                                             Optional relationshipDirection As Core.enumRelationshipDirection = Core.enumRelationshipDirection.source,
                                             Optional typeId As String = Nothing,
                                             Optional maxItems As xs_Integer? = Nothing,
                                             Optional skipCount As xs_Integer? = Nothing,
                                             Optional filter As String = Nothing,
                                             Optional includeAllowableActions As Boolean? = Nothing) As Generic.ItemList(Of CmisObject)
         With _client.GetObjectRelationships(New cmr.getObjectRelationships() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                                                    .IncludeSubRelationshipTypes = includeSubRelationshipTypes,
                                                                                    .RelationshipDirection = relationshipDirection, .TypeId = typeId,
                                                                                    .MaxItems = maxItems, .SkipCount = skipCount, .Filter = filter,
                                                                                    .IncludeAllowableActions = includeAllowableActions})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return Convert(.Response.Objects)
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "Policy"
      ''' <summary>
      ''' Applies a specified policy to the current object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ApplyPolicy(policyId As String) As Boolean
         With _client.ApplyPolicy(New cmr.applyPolicy() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId, .PolicyId = policyId})
            _lastException = .Exception
            Return _lastException Is Nothing
         End With
      End Function

      ''' <summary>
      ''' Gets the list of policies currently applied to the current object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetAppliedPolicies(Optional filter As String = Nothing) As CmisPolicy()
         With _client.GetAppliedPolicies(New cmr.getAppliedPolicies() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId, .Filter = filter})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return (From [object] As Core.cmisObjectType In .Response.Objects
                       Let policy As CmisPolicy = TryCast(CreateCmisObject([object]), CmisPolicy)
                       Where policy IsNot Nothing
                       Select policy).ToArray()
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Removes a specified policy from the current object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function RemovePolicy(policyId As String) As Boolean
         With _client.RemovePolicy(New cmr.removePolicy() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId, .PolicyId = policyId})
            _lastException = .Exception
            Return _lastException Is Nothing
         End With
      End Function
#End Region

#Region "Acl"
      ''' <summary>
      ''' Adds or removes the given ACEs to or from the ACL of an object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ApplyAcl(addACEs As Core.Security.cmisAccessControlListType, removeACEs As Core.Security.cmisAccessControlListType,
                               Optional aclPropagation As Core.enumACLPropagation = Core.enumACLPropagation.repositorydetermined) As Messaging.cmisACLType
         With _client.ApplyAcl(New cmr.applyACL() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId,
                                                        .AddACEs = addACEs, .RemoveACEs = removeACEs, .ACLPropagation = aclPropagation})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return .Response.ACL
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Get the ACL currently applied to the specified object
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetAcl(Optional onlyBasicPermissions As Boolean = True) As Messaging.cmisACLType
         With _client.GetAcl(New cmr.getACL() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = _cmisObject.ObjectId, .OnlyBasicPermissions = onlyBasicPermissions})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return .Response.ACL
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "AllowableActions"
      Public Property CanAddObjectToFolder As Boolean
         Get
            If _cmisObject.AllowableActions Is Nothing Then
               Return False
            Else
               With _cmisObject.AllowableActions.CanAddObjectToFolder

                  Return .HasValue AndAlso .Value
               End With
            End If
         End Get
         Set(value As Boolean)
            If _cmisObject.AllowableActions Is Nothing Then _cmisObject.AllowableActions.CanAddObjectToFolder = value
         End Set
      End Property
#End Region

      Protected _cmisObject As Core.cmisObjectType
      Public ReadOnly Property [Object] As Core.cmisObjectType
         Get
            Return _cmisObject
         End Get
      End Property

      ''' <summary>
      ''' Access to properties via index or PropertyDefinitionId
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property PropertiesAsReadOnly As CmisObjectModel.Collections.Generic.ArrayMapper(Of Core.Collections.cmisPropertiesType, Core.Properties.cmisProperty)
         Get
            Return _cmisObject.PropertiesAsReadOnly
         End Get
      End Property

      Public Property PathSegment As String
      Public Property RelativePathSegment As String

      ''' <summary>
      ''' Returns the CmisType for specified typeId
      ''' </summary>
      ''' <param name="typeId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overrides Function GetTypeDefinition(typeId As String) As CmisType
         Return If(String.IsNullOrEmpty(typeId), Nothing, MyBase.GetTypeDefinition(typeId))
      End Function

   End Class
End Namespace