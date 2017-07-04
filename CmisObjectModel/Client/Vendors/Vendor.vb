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
Imports ca = CmisObjectModel.AtomPub
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports ccg = CmisObjectModel.Client.Generic
Imports ce = CmisObjectModel.Extensions
Imports cea = CmisObjectModel.Extensions.Alfresco
Imports cmr = CmisObjectModel.Messaging.Requests
Imports sc = System.ComponentModel
Imports ss = System.ServiceModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Client.Vendors
   ''' <summary>
   ''' Base class for all vendor-extensions
   ''' </summary>
   ''' <remarks>
   ''' Support 
   ''' </remarks>
   Public Class Vendor

      Protected Shared _objectTypeIdKey As String = CmisPredefinedPropertyNames.ObjectTypeId.ToLowerInvariant()
      Protected _client As Contracts.ICmisClient
      Public Sub New(client As Contracts.ICmisClient)
         _client = client
      End Sub

#Region "Helper classes"
      Public Class State

#Region "Constructors"
         Public Sub New(repositoryId As String)
            Me.RepositoryId = repositoryId
         End Sub

         Public Sub New(repositoryId As String, typeId As String)
            Me.RepositoryId = repositoryId
            If Not String.IsNullOrEmpty(typeId) Then
               Dim typeIds As String() = typeId.Split(","c)
               Dim length As Integer = typeIds.Length - 1

               Me.TypeId = typeIds(0)
               If length > 0 Then
                  Me.SecondaryTypeIds = CType(Array.CreateInstance(GetType(String), length), String())
                  Array.Copy(typeIds, 1, Me.SecondaryTypeIds, 0, length)
               End If
            End If
         End Sub
#End Region

         ''' <summary>
         ''' Adds a new delegate to rollback changes
         ''' </summary>
         ''' <param name="value"></param>
         ''' <remarks></remarks>
         Public Sub AddRollbackAction(value As Action)
            If value IsNot Nothing Then _rollbackActions.Push(value)
         End Sub

         Private _rollbackActions As New Stack(Of Action)
         ''' <summary>
         ''' Executes given rollback-actions (via AddRollbackAction()) in reverse order
         ''' </summary>
         ''' <remarks></remarks>
         Public Sub Rollback()
            While _rollbackActions.Count > 0
               _rollbackActions.Pop().Invoke()
            End While
         End Sub

         Public ReadOnly SecondaryTypeIds As String()
         Public ReadOnly TypeId As String

         Public ReadOnly RepositoryId As String
      End Class
#End Region

#Region "SecondaryObjectType-Support"
      ''' <summary>
      ''' Selects from secondaryTypes the typeIds defined in secondaryTypeFilter and adds their propertyDefinitions
      ''' to the result parameter, respecting the optional given propertyFilter
      ''' </summary>
      ''' <param name="secondaryTypes"></param>
      ''' <param name="secondaryTypeFilter"></param>
      ''' <param name="propertyFilter">Set of supported propertyDefinitionId-namespaces.
      ''' If filter is set to null or contains '*' all namespaces are accepted.</param>
      ''' <remarks></remarks>
      Protected Function AddSecondaryProperties(result As Dictionary(Of String, Core.Definitions.Properties.cmisPropertyDefinitionType),
                                                secondaryTypes As Dictionary(Of String, Core.Definitions.Types.cmisTypeDefinitionType),
                                                secondaryTypeFilter As String(),
                                                Optional propertyFilter As HashSet(Of String) = Nothing) As Dictionary(Of String, Core.Definitions.Properties.cmisPropertyDefinitionType)
         'collect propertyDefinitions from selected types
         For Each typeId As String In secondaryTypeFilter
            If Not String.IsNullOrEmpty(typeId) AndAlso secondaryTypes.ContainsKey(typeId) Then
               Dim propertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType() = secondaryTypes(typeId).PropertyDefinitions

               If propertyDefinitions IsNot Nothing Then
                  For Each propertyDefinition As Core.Definitions.Properties.cmisPropertyDefinitionType In propertyDefinitions
                     Dim key As String = propertyDefinition.Id.ToLowerInvariant()

                     If Not result.ContainsKey(key) Then
                        If propertyFilter Is Nothing OrElse propertyFilter.Contains("*") Then
                           'all prefixes allowed
                           result.Add(key, propertyDefinition)
                        Else
                           'check for defined prefixes
                           Dim indexOf As Integer = key.IndexOf(":")
                           If propertyFilter.Contains(If(indexOf <= 0, "", key.Substring(0, indexOf))) Then
                              result.Add(key, propertyDefinition)
                           End If
                        End If
                     End If
                  Next
               End If
            End If
         Next

         Return result
      End Function

      ''' <summary>
      ''' First key: repositoryId, second key: secondaryObjectType identifier
      ''' </summary>
      ''' <remarks></remarks>
      Protected _secondaryObjectTypes As New Dictionary(Of String, Dictionary(Of String, ccdt.cmisTypeDefinitionType))
      Protected Function GetSecondaryObjectTypes(repositoryId As String) As Dictionary(Of String, ccdt.cmisTypeDefinitionType)
         SyncLock _secondaryObjectTypes
            If _secondaryObjectTypes.ContainsKey(repositoryId) Then
               Return _secondaryObjectTypes(repositoryId)
            Else
               Dim retVal As Dictionary(Of String, ccdt.cmisTypeDefinitionType) =
                  GetTypeDescendants(repositoryId, "cmis:secondary")
               _secondaryObjectTypes.Add(repositoryId, retVal)

               Return retVal
            End If
         End SyncLock
      End Function

      ''' <summary>
      ''' Collects all propertyDefinition of the secondary types (secondaryObjectTypes)
      ''' </summary>
      ''' <param name="state"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function GetSecondaryTypeProperties(state As State) As Dictionary(Of String, Core.Definitions.Properties.cmisPropertyDefinitionType)
         Return AddSecondaryProperties(New Dictionary(Of String, Core.Definitions.Properties.cmisPropertyDefinitionType),
                                       GetSecondaryObjectTypes(state.RepositoryId), state.SecondaryTypeIds)
      End Function

      ''' <summary>
      ''' Returns type definitions derived (directly or indirectly) from parentTypeId
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="parentTypeId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function GetTypeDescendants(repositoryId As String, parentTypeId As String) As Dictionary(Of String, Core.Definitions.Types.cmisTypeDefinitionType)
         Dim retVal As New Dictionary(Of String, ccdt.cmisTypeDefinitionType)

         With _client.GetTypeDescendants(New cmr.getTypeDescendants() With {.RepositoryId = repositoryId, .TypeId = parentTypeId, .IncludePropertyDefinitions = True})
            If .Exception Is Nothing AndAlso .Response.Types IsNot Nothing Then
               Dim types As New Queue(Of Messaging.cmisTypeContainer())

               'first level
               types.Enqueue(.Response.Types)
               While types.Count > 0
                  For Each cmisTypeContainer As Messaging.cmisTypeContainer In types.Dequeue
                     If cmisTypeContainer IsNot Nothing Then
                        Dim cmisType As Core.Definitions.Types.cmisTypeDefinitionType = cmisTypeContainer.Type
                        Dim children As Messaging.cmisTypeContainer() = cmisTypeContainer.Children

                        If cmisType IsNot Nothing Then
                           Dim id As String = cmisType.Id

                           If Not String.IsNullOrEmpty(id) Then
                              Dim keys As String() = New String() {id, id.ToLowerInvariant(), id.ToUpperInvariant()}

                              For Each key As String In keys
                                 If Not retVal.ContainsKey(key) Then retVal.Add(key, cmisType)
                              Next
                           End If
                           'children must be registered as well (next level)
                           If children IsNot Nothing Then types.Enqueue(children)
                        End If
                     End If
                  Next
               End While
            End If
         End With

         Return retVal
      End Function
#End Region

#Region "Vendor specific for cmisPropertyCollectionType"
      Public Function BeginRequest(repositoryId As String) As State
         Return BeginRequest(New State(repositoryId, Nothing), Nothing)
      End Function
      ''' <summary>
      ''' Allows vendor specific action before request dealing with cmisObjects
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="properties"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function BeginRequest(repositoryId As String, properties As Core.Collections.cmisPropertiesType,
                                   rollbackAction As Action) As State
         Dim objectTypeIdProperty As Core.Properties.cmisProperty = If(properties Is Nothing, Nothing,
                                                                       properties.FindProperty(CmisPredefinedPropertyNames.ObjectTypeId))
         Dim typeId As String = If(objectTypeIdProperty Is Nothing, Nothing, CStr(objectTypeIdProperty.Value))
         Dim state As New State(repositoryId, typeId)

         state.AddRollbackAction(rollbackAction)
         'remove secondaryTypeIds
         If state.SecondaryTypeIds IsNot Nothing Then
            Dim oldTypeId As Object = objectTypeIdProperty.SetValueSilent(state.TypeId)
            state.AddRollbackAction(Sub()
                                       objectTypeIdProperty.SetValueSilent(oldTypeId)
                                    End Sub)
         End If
         Return BeginRequest(state, properties)
      End Function
      Protected Overridable Function BeginRequest(state As State, properties As Core.Collections.cmisPropertiesType) As State
         Return state
      End Function

      ''' <summary>
      ''' Allows vendor specific action to process the response dealing with cmisObjects
      ''' </summary>
      ''' <param name="state"></param>
      ''' <param name="propertyCollections"></param>
      ''' <remarks></remarks>
      Public Overridable Sub EndRequest(state As State, ParamArray propertyCollections As Core.Collections.cmisPropertiesType())
      End Sub
#End Region

#Region "Vendor specific for cmisTypeDefinitionType"
      ''' <summary>
      ''' Allows vendor specific action before request dealing with cmisTypes
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="typeId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function BeginRequest(repositoryId As String, ByRef typeId As String) As State
         Dim retVal As New State(repositoryId, typeId)

         'the given typeId is a composite typeId, starting with the original typeId followed by secondaryTypeIds
         If retVal.SecondaryTypeIds IsNot Nothing Then typeId = retVal.TypeId

         Return retVal
      End Function

      ''' <summary>
      ''' Allows vendor specific action to process the response dealing with cmisTypes
      ''' </summary>
      ''' <param name="state"></param>
      ''' <param name="type"></param>
      ''' <remarks></remarks>
      Public Overridable Sub EndRequest(state As State, type As Core.Definitions.Types.cmisTypeDefinitionType)
         Dim secondaryTypeProperties = If(state.SecondaryTypeIds Is Nothing, Nothing, GetSecondaryTypeProperties(state))

         If secondaryTypeProperties IsNot Nothing Then
            Dim propertyDefinitions As Dictionary(Of String, Core.Definitions.Properties.cmisPropertyDefinitionType) =
                  type.GetPropertyDefinitions(True)
            Dim count As Integer = propertyDefinitions.Count

            For Each de As KeyValuePair(Of String, Core.Definitions.Properties.cmisPropertyDefinitionType) In secondaryTypeProperties
               If Not propertyDefinitions.ContainsKey(de.Key) Then
                  propertyDefinitions.Add(de.Key, de.Value)
               End If
            Next
            'at least one aspect property has been added
            If count <> propertyDefinitions.Count Then
               type.PropertyDefinitions = propertyDefinitions.Values.ToArray()
            End If
         End If
      End Sub
#End Region

#Region "Patches"
      ''' <summary>
      ''' Allows vendor specific action to patch values within the propertyCollection
      ''' </summary>
      ''' <remarks>
      ''' For example: in Agorum the cmis:versionSeriesId of a pwc differs from the cmis:versionSeriesId
      ''' of the checkedin-versions. High level code in this assembly (CmisObjectModel.Client.CmisDataModelObject)
      ''' uses this method to make sure the cmis:versionSeriesId of both are the same.
      ''' </remarks>
      Public Overridable Sub PatchProperties(repositoryInfo As Core.cmisRepositoryInfoType, properties As Core.Collections.cmisPropertiesType)
      End Sub

      Public Overridable Sub PatchProperties(repositoryInfo As Core.cmisRepositoryInfoType, cmisObject As Core.cmisObjectType)
         If cmisObject IsNot Nothing Then PatchProperties(repositoryInfo, cmisObject.Properties)
      End Sub
#End Region

   End Class
End Namespace