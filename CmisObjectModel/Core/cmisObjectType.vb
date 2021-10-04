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
Imports ccg = CmisObjectModel.Common.Generic
Imports sss = System.ServiceModel.Syndication
Imports sxl = System.Xml.Linq
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core
   Partial Public Class cmisObjectType

      Private _predefinedPropertyDefinitionFactory As New PredefinedPropertyDefinitionFactory(Nothing)
      Public Sub New(ParamArray properties As Core.Properties.cmisProperty())
         If properties IsNot Nothing AndAlso properties.Length > 0 Then
            _properties = New Core.Collections.cmisPropertiesType(properties)
         Else
            _properties = New Collections.cmisPropertiesType()
         End If
         InitClass()
      End Sub

      Public Sub New(properties As Core.Collections.cmisPropertiesType)
         If properties IsNot Nothing Then _properties = New Collections.cmisPropertiesType() With {.Extensions = properties.Extensions, .Properties = properties.Properties}
         InitClass()
      End Sub

      Protected Overrides Sub InitClass()
         MyBase.InitClass()
      End Sub

#Region "Helper-classes"
      ''' <summary>
      ''' Compares two cmisObjectType-instances by comparisons of the selected properties
      ''' </summary>
      ''' <remarks></remarks>
      Public Class cmisObjectTypeComparer
         Implements IComparable(Of cmisObjectTypeComparer)

         Public Sub New(cmisObject As cmisObjectType, propertyNames As String())
            _cmisObject = cmisObject
            _propertyNames = propertyNames
         End Sub

#Region "IComparable"
         Public Function CompareTo(other As cmisObjectTypeComparer) As Integer Implements System.IComparable(Of cmisObjectTypeComparer).CompareTo
            Dim properties As CmisObjectModel.Collections.Generic.ArrayMapper(Of Collections.cmisPropertiesType, Core.Properties.cmisProperty) =
               If(_cmisObject Is Nothing, Nothing, _cmisObject.PropertiesAsReadOnly)
            Dim otherProperties As CmisObjectModel.Collections.Generic.ArrayMapper(Of Collections.cmisPropertiesType, Core.Properties.cmisProperty) =
               If(other Is Nothing OrElse other._cmisObject Is Nothing, Nothing, other._cmisObject.PropertiesAsReadOnly)

            If otherProperties Is Nothing Then
               Return If(properties Is Nothing, 0, 1)
            ElseIf properties Is Nothing Then
               Return -1
            Else
               Dim length As Integer = If(_propertyNames Is Nothing, 0, _propertyNames.Length)
               Dim otherLength As Integer = If(other._propertyNames Is Nothing, 0, other._propertyNames.Length)

               If otherLength = 0 Then
                  Return If(length = 0, 0, 1)
               ElseIf length = 0 Then
                  Return -1
               Else
                  For index As Integer = 0 To Math.Min(length, otherLength) - 1
                     Dim result As Integer = Core.Properties.cmisProperty.Compare(properties.Item(_propertyNames(index)),
                                                                                  otherProperties.Item(other._propertyNames(index)))
                     If result <> 0 Then Return result
                  Next
                  Return If(length = otherLength, 0, If(length < otherLength, -1, 1))
               End If
            End If
         End Function
#End Region

         Private _cmisObject As cmisObjectType
         Private _propertyNames As String()

      End Class
#End Region

#Region "predefined properties"
      Private Function GetPropertyValue(Of T)(id As String) As T
         Dim p As Properties.cmisProperty = _properties.GetByPropertyDefinitionId(id)
         If p Is Nothing OrElse p.Value Is Nothing Then Return Nothing
         Return DirectCast(p.Value, T)
      End Function
      Private Function GetPropertyValues(Of T)(id As String) As T()
         Dim p As Properties.cmisProperty = _properties.GetByPropertyDefinitionId(id)
         If p Is Nothing OrElse p.Values Is Nothing Then Return Nothing
         Return (From v In p.Values Select DirectCast(v, T)).ToArray()
      End Function
      Private Sub SetPropertyValue(Of T)(id As String, name As String, value As T, createPropertyFunc As Func(Of T, Properties.cmisProperty))
         Dim p As Properties.cmisProperty = _properties.GetByPropertyDefinitionId(id)
         If p Is Nothing Then
            _properties.AddProperty(createPropertyFunc(value))
         Else
            Dim oldValue = p.Value
            p.Value = value
            OnPropertyChanged(name, value, oldValue)
         End If
      End Sub

      Private Sub SetPropertyValues(id As String, name As String, value As Object(), createPropertyFunc As Func(Of Object(), Properties.cmisProperty))
         Dim p As Properties.cmisProperty = _properties.GetByPropertyDefinitionId(id)
         If p Is Nothing Then
            _properties.AddProperty(createPropertyFunc(value))
         Else
            Dim oldValue = p.Value
            p.Values = value
            OnPropertyChanged(name, value, oldValue)
         End If
      End Sub

      Public Property AllowedChildObjectTypeIds As ccg.Nullable(Of String())
         Get
            Return GetPropertyValues(Of String)(CmisPredefinedPropertyNames.AllowedChildObjectTypeIds)
         End Get
         Set(value As ccg.Nullable(Of String()))
            SetPropertyValues(CmisPredefinedPropertyNames.AllowedChildObjectTypeIds, "AllowedChildObjectTypeIds", value, Function(v) _predefinedPropertyDefinitionFactory.AllowedChildObjectTypeIds.CreateProperty(v))
         End Set
      End Property 'AllowedChildObjectTypeIds

      Public Property BaseTypeId As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.BaseTypeId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.BaseTypeId, "BaseTypeId", value, Function(v) _predefinedPropertyDefinitionFactory.BaseTypeId.CreateProperty(v))
         End Set
      End Property 'BaseTypeId

      Public Property ChangeToken As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.ChangeToken)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.ChangeToken, "ChangeToken", value, Function(v) _predefinedPropertyDefinitionFactory.ChangeToken.CreateProperty(v))
         End Set
      End Property 'ChangeToken

      Public Property CheckinComment As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.CheckinComment)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.CheckinComment, "CheckinComment", value, Function(v) _predefinedPropertyDefinitionFactory.CheckinComment.CreateProperty(v))
         End Set
      End Property 'CheckinComment

      Public Property ContentStreamFileName As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.ContentStreamFileName)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.ContentStreamFileName, "ContentStreamFileName", value, Function(v) _predefinedPropertyDefinitionFactory.ContentStreamFileName.CreateProperty(v))
         End Set
      End Property 'ContentStreamFileName

      Public Property ContentStreamId As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.ContentStreamId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.ContentStreamId, "ContentStreamId", value, Function(v) _predefinedPropertyDefinitionFactory.ContentStreamId.CreateProperty(v))
         End Set
      End Property 'ContentStreamId

      Public Property ContentStreamLength As xs_Integer?
         Get
            Return GetPropertyValue(Of xs_Integer?)(CmisPredefinedPropertyNames.ContentStreamLength)
         End Get
         Set(value As xs_Integer?)
            SetPropertyValue(Of xs_Integer?)(CmisPredefinedPropertyNames.ContentStreamLength, "ContentStreamLength", value, Function(v) _predefinedPropertyDefinitionFactory.ContentStreamLength.CreateProperty(v))
         End Set
      End Property 'ContentStreamLength

      Public Property ContentStreamMimeType As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.ContentStreamMimeType)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.ContentStreamMimeType, "ContentStreamMimeType", value, Function(v) _predefinedPropertyDefinitionFactory.ContentStreamMimeType.CreateProperty(v))
         End Set
      End Property 'ContentStreamMimeType

      Public Property CreatedBy As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.CreatedBy)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.CreatedBy, "CreatedBy", value, Function(v) _predefinedPropertyDefinitionFactory.CreatedBy.CreateProperty(v))
         End Set
      End Property 'CreatedBy

      Public Property CreationDate As DateTimeOffset?
         Get
            Return GetPropertyValue(Of DateTimeOffset?)(CmisPredefinedPropertyNames.CreationDate)
         End Get
         Set(value As DateTimeOffset?)
            SetPropertyValue(Of DateTimeOffset?)(CmisPredefinedPropertyNames.CreationDate, "CreationDate", value, Function(v) _predefinedPropertyDefinitionFactory.CreationDate.CreateProperty(v))
         End Set
      End Property 'CreationDate

      Public Property Description As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.Description)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.Description, "Description", value, Function(v) _predefinedPropertyDefinitionFactory.Description.CreateProperty(v))
         End Set
      End Property 'Description

      Public Property IsImmutable As Boolean?
         Get
            Return GetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsImmutable)
         End Get
         Set(value As Boolean?)
            SetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsImmutable, "IsImmutable", value, Function(v) _predefinedPropertyDefinitionFactory.IsImmutable.CreateProperty(v))
         End Set
      End Property 'IsImmutable

      Public Property IsLatestMajorVersion As Boolean?
         Get
            Return GetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsLatestMajorVersion)
         End Get
         Set(value As Boolean?)
            SetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsLatestMajorVersion, "IsLatestMajorVersion", value, Function(v) _predefinedPropertyDefinitionFactory.IsLatestMajorVersion.CreateProperty(v))
         End Set
      End Property 'IsLatestMajorVersion

      Public Property IsLatestVersion As Boolean?
         Get
            Return GetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsLatestVersion)
         End Get
         Set(value As Boolean?)
            SetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsLatestVersion, "IsLatestVersion", value, Function(v) _predefinedPropertyDefinitionFactory.IsLatestVersion.CreateProperty(v))
         End Set
      End Property 'IsLatestVersion

      Public Property IsMajorVersion As Boolean?
         Get
            Return GetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsMajorVersion)
         End Get
         Set(value As Boolean?)
            SetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsMajorVersion, "IsMajorVersion", value, Function(v) _predefinedPropertyDefinitionFactory.IsMajorVersion.CreateProperty(v))
         End Set
      End Property 'IsMajorVersion

      Private Shared Sub SetIsMajorVersion(instance As cmisObjectType, value As Boolean?)
         instance.IsMajorVersion = value
      End Sub

      Public Property IsPrivateWorkingCopy As Boolean?
         Get
            Return GetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsPrivateWorkingCopy)
         End Get
         Set(value As Boolean?)
            SetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsPrivateWorkingCopy, "IsPrivateWorkingCopy", value, Function(v) _predefinedPropertyDefinitionFactory.IsPrivateWorkingCopy.CreateProperty(v))
         End Set
      End Property 'IsPrivateWorkingCopy

      Public Property IsVersionSeriesCheckedOut As Boolean?
         Get
            Return GetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut)
         End Get
         Set(value As Boolean?)
            SetPropertyValue(Of Boolean?)(CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut, "IsVersionSeriesCheckedOut", value, Function(v) _predefinedPropertyDefinitionFactory.IsVersionSeriesCheckedOut.CreateProperty(v))
         End Set
      End Property 'IsVersionSeriesCheckedOut

      Public Property LastModificationDate As DateTimeOffset?
         Get
            Return GetPropertyValue(Of DateTimeOffset?)(CmisPredefinedPropertyNames.LastModificationDate)
         End Get
         Set(value As DateTimeOffset?)
            SetPropertyValue(Of DateTimeOffset?)(CmisPredefinedPropertyNames.LastModificationDate, "LastModificationDate", value, Function(v) _predefinedPropertyDefinitionFactory.LastModificationDate.CreateProperty(v))
         End Set
      End Property 'LastModificationDate

      Public Property LastModifiedBy As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.LastModifiedBy)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.LastModifiedBy, "LastModifiedBy", value, Function(v) _predefinedPropertyDefinitionFactory.LastModifiedBy.CreateProperty(v))
         End Set
      End Property 'LastModifiedBy

      Public Property Name As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.Name)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.Name, "Name", value, Function(v) _predefinedPropertyDefinitionFactory.Name.CreateProperty(v))
         End Set
      End Property 'Name

      Public Property ObjectId() As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.ObjectId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.ObjectId, "ObjectId", value, Function(v) _predefinedPropertyDefinitionFactory.ObjectId.CreateProperty(v))
         End Set
      End Property 'ObjectId

      Public Property ObjectTypeId As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.ObjectTypeId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.ObjectTypeId, "ObjectTypeId", value, Function(v) _predefinedPropertyDefinitionFactory.ObjectTypeId.CreateProperty(v))
         End Set
      End Property 'ObjectTypeId

      Public Property ParentId As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.ParentId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.ParentId, "ParentId", value, Function(v) _predefinedPropertyDefinitionFactory.ParentId.CreateProperty(v))
         End Set
      End Property 'ParentId


      Public Property Path As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.Path)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.Path, "Path", value, Function(v) _predefinedPropertyDefinitionFactory.Path.CreateProperty(v))
         End Set
      End Property 'Path

      Public Property PolicyText As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.PolicyText)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.PolicyText, "PolicyText", value, Function(v) _predefinedPropertyDefinitionFactory.PolicyText.CreateProperty(v))
         End Set
      End Property 'PolicyText

      Public Property SecondaryObjectTypeIds As ccg.Nullable(Of String())
         Get
            Return GetPropertyValues(Of String)(CmisPredefinedPropertyNames.SecondaryObjectTypeIds)
         End Get
         Set(value As ccg.Nullable(Of String()))
            SetPropertyValues(CmisPredefinedPropertyNames.SecondaryObjectTypeIds, "SecondaryObjectTypeIds", value, Function(v) _predefinedPropertyDefinitionFactory.SecondaryObjectTypeIds.CreateProperty(v))
         End Set
      End Property 'SecondaryObjectTypeIds

      Public Property SourceId As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.SourceId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.SourceId, "SourceId", value, Function(v) _predefinedPropertyDefinitionFactory.SourceId.CreateProperty(v))
         End Set
      End Property 'SourceId

      Public Property TargetId As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.TargetId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.TargetId, "TargetId", value, Function(v) _predefinedPropertyDefinitionFactory.TargetId.CreateProperty(v))
         End Set
      End Property 'TargetId

      Public Property VersionLabel As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.VersionLabel)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.VersionLabel, "VersionLabel", value, Function(v) _predefinedPropertyDefinitionFactory.VersionLabel.CreateProperty(v))
         End Set
      End Property 'VersionLabel

      Public Property VersionSeriesCheckedOutBy As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy, "VersionSeriesCheckedOutBy", value, Function(v) _predefinedPropertyDefinitionFactory.VersionSeriesCheckedOutBy.CreateProperty(v))
         End Set
      End Property 'VersionSeriesCheckedOutBy

      Public Property VersionSeriesCheckedOutId As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.VersionSeriesCheckedOutId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.VersionSeriesCheckedOutId, "VersionSeriesCheckedOutId", value, Function(v) _predefinedPropertyDefinitionFactory.VersionSeriesCheckedOutId.CreateProperty(v))
         End Set
      End Property 'VersionSeriesCheckedOutId

      Public Property VersionSeriesId As ccg.Nullable(Of String)
         Get
            Return GetPropertyValue(Of String)(CmisPredefinedPropertyNames.VersionSeriesId)
         End Get
         Set(value As ccg.Nullable(Of String))
            SetPropertyValue(Of String)(CmisPredefinedPropertyNames.VersionSeriesId, "VersionSeriesId", value, Function(v) _predefinedPropertyDefinitionFactory.VersionSeriesId.CreateProperty(v))
         End Set
      End Property 'VersionSeriesId

#End Region

#Region "IXmlSerializable"
      Public Overrides Sub ReadXml(reader As System.Xml.XmlReader)
         MyBase.ReadXml(reader)
         'RefreshObservation()
      End Sub
#End Region

#Region "general links of a cmisObjectType-instance"
      ''' <summary>
      ''' Creates a list of links for a cmisObjectType-instance
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="elementFactory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function GetLinks(Of TLink)(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                            elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate) As List(Of TLink)
         Dim repositoryId As String = repositoryInfo.RepositoryId
         Dim objId As String = ObjectId
         Dim objTypeId As String = ObjectTypeId
         Dim retVal As New List(Of TLink) From {
            elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryId, "id", objId)),
                           LinkRelationshipTypes.Self, MediaTypes.Entry, objId, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.GetRepositoryInfo.ReplaceUri("repositoryId", repositoryId)),
                           LinkRelationshipTypes.Service, MediaTypes.Service, Nothing, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", repositoryId, "id", objTypeId)),
                           LinkRelationshipTypes.DescribedBy, MediaTypes.Entry, Nothing, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.GetAllowableActions.ReplaceUri("repositoryId", repositoryId, "id", objId)),
                           LinkRelationshipTypes.AllowableActions, MediaTypes.AllowableActions, Nothing, Nothing)}

         If repositoryInfo.Capabilities.CapabilityACL <> enumCapabilityACL.none Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.GetAcl.ReplaceUri("repositoryId", repositoryId, "id", objId)),
                       LinkRelationshipTypes.Acl, MediaTypes.Acl, Nothing, Nothing))
         End If
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.PoliciesUri(ServiceURIs.enumPoliciesUri.objectId).ReplaceUri("repositoryId", repositoryId, "id", objId)),
                    LinkRelationshipTypes.Policies, MediaTypes.Feed, Nothing, Nothing))

         Return retVal
      End Function

      ''' <summary>
      ''' Creates a list of links for a cmisObjectType-instance
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType) As List(Of AtomPub.AtomLink)
         Return GetLinks(Of AtomPub.AtomLink)(baseUri, repositoryInfo,
                                              Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind))
      End Function
      ''' <summary>
      ''' Creates a list of links for a cmisObjectType-instance
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                               ns As sxl.XNamespace, elementName As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetLinks(Of sxl.XElement)(baseUri, repositoryInfo,
                                             AddressOf .CreateXElement)
         End With
      End Function
#End Region

#Region "links of a cmisdocument"
      ''' <summary>
      ''' Returns links for a cmisdocument
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="elementFactory"></param>
      ''' <param name="canGetAllVersions"></param>
      ''' <param name="isLatestVersion"></param>
      ''' <param name="originId">ObjectId of the document (current version).
      ''' The parameter differs from the Id-property if this instance references to the private working copy</param>
      ''' <param name="privateWorkingCopyId">ObjectId of the private working copy if existing.
      ''' The parameter differs from the Id-property if this instance doesn't reference to the private working copy</param>
      ''' <returns></returns>
      ''' <remarks>Used Link Relations:
      ''' 3.11.2 Document Entry, 3.11.3 PWC Entry; 3.4.3 CMIS Link Relations</remarks>
      Protected Function GetDocumentLinks(Of TLink)(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                                    elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate,
                                                    canGetAllVersions As Boolean, isLatestVersion As Boolean,
                                                    originId As String, privateWorkingCopyId As String) As List(Of TLink)
         Dim objId As String = ObjectId
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)
         Dim isWorkingCopy As Boolean = Not (String.IsNullOrEmpty(privateWorkingCopyId) OrElse objId <> privateWorkingCopyId)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectParentsUri(ServiceURIs.enumObjectParentsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Up, MediaTypes.Feed, Nothing, Nothing))
         If canGetAllVersions Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.AllVersionsUri(ServiceURIs.enumAllVersionsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                      LinkRelationshipTypes.VersionHistory, MediaTypes.Feed, Nothing, Nothing))
         End If
         If Not (isLatestVersion OrElse isWorkingCopy) Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.specifyVersion).ReplaceUri("returnVersion", GetName(RestAtom.enumReturnVersion.latest), "repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                      LinkRelationshipTypes.CurrentVersion, MediaTypes.Entry, Nothing, Nothing))
         End If
         If repositoryInfo.Capabilities.CapabilityContentStreamUpdatability <> Core.enumCapabilityContentStreamUpdates.none Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ContentUri(ServiceURIs.enumContentUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                      LinkRelationshipTypes.EditMedia, MediaTypes.Stream, Nothing, Nothing))
         End If
         If Not String.IsNullOrEmpty(privateWorkingCopyId) Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.workingCopy).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", privateWorkingCopyId, "pwc", "true")),
                                      LinkRelationshipTypes.WorkingCopy, MediaTypes.Entry, privateWorkingCopyId, Nothing))
         End If
         If _renditions IsNot Nothing AndAlso _renditions.Count > 0 Then
            For Each rendition As cmisRenditionType In _renditions
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ContentUri(ServiceURIs.enumContentUri.getContentStream).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId, "streamId", rendition.StreamId)),
                                         LinkRelationshipTypes.Alternate, rendition.Mimetype, Nothing, rendition.Kind))
            Next
         End If
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Relationships, MediaTypes.Feed, Nothing, Nothing))

         If isWorkingCopy AndAlso Not String.IsNullOrEmpty(originId) Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", originId)),
                                      LinkRelationshipTypes.Via, MediaTypes.Entry, originId, Nothing))
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Returns links for a cmisdocument in System.ServiceModel.Syndication.SyndicationLink-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="canGetAllVersions"></param>
      ''' <param name="isLatestVersion"></param>
      ''' <param name="originId">ObjectId of the document (current version).
      ''' The parameter differs from the Id-property if this instance references to the private working copy</param>
      ''' <param name="privateWorkingCopyId">ObjectId of the private working copy if existing.
      ''' The parameter differs from the Id-property if this instance doesn't reference to the private working copy</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDocumentLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                       canGetAllVersions As Boolean, isLatestVersion As Boolean,
                                       originId As String, privateWorkingCopyId As String) As List(Of AtomPub.AtomLink)
         Return GetDocumentLinks(Of AtomPub.AtomLink)(baseUri, repositoryInfo,
                                                      Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind),
                                                      canGetAllVersions, isLatestVersion,
                                                      originId, privateWorkingCopyId)
      End Function
      ''' <summary>
      ''' Returns links for a cmisdocument in System.Xml.Linq.XElement-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <param name="canGetAllVersions"></param>
      ''' <param name="isLatestVersion"></param>
      ''' <param name="originId">ObjectId of the document (current version).
      ''' The parameter differs from the Id-property if this instance references to the private working copy</param>
      ''' <param name="privateWorkingCopyId">ObjectId of the private working copy if existing.
      ''' The parameter differs from the Id-property if this instance doesn't reference to the private working copy</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDocumentLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                       ns As sxl.XNamespace, elementName As String,
                                       canGetAllVersions As Boolean, isLatestVersion As Boolean,
                                       originId As String, privateWorkingCopyId As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetDocumentLinks(Of sxl.XElement)(baseUri, repositoryInfo,
                                                     AddressOf .CreateXElement,
                                                     canGetAllVersions, isLatestVersion, originId, privateWorkingCopyId)
         End With
      End Function
#End Region

#Region "links for a cmisfolder"
      ''' <summary>
      ''' Returns links for a cmisfolder
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="elementFactory"></param>
      ''' <param name="parentId"></param>
      ''' <returns></returns>
      ''' <remarks>Used Link Relations:
      ''' 3.11.4 Folder Entry; 3.4.3 CMIS Link Relations</remarks>
      Protected Function GetFolderLinks(Of TLink)(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                                  elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate,
                                                  parentId As String) As List(Of TLink)
         Dim objId As String = ObjectId
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ChildrenUri(ServiceURIs.enumChildrenUri.folderId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Down, MediaTypes.Feed, Nothing, Nothing))
         If repositoryInfo.Capabilities.CapabilityGetDescendants Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.DescendantsUri(ServiceURIs.enumDescendantsUri.folderId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                      LinkRelationshipTypes.Down, MediaTypes.Tree, Nothing, Nothing))
         End If
         If Not String.IsNullOrEmpty(parentId) Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", parentId)),
                                      LinkRelationshipTypes.Up, MediaTypes.Entry, parentId, Nothing))
         End If
         If _renditions IsNot Nothing AndAlso _renditions.Count > 0 Then
            For Each rendition As cmisRenditionType In _renditions
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                         LinkRelationshipTypes.Alternate, rendition.Mimetype, Nothing, rendition.Kind))
            Next
         End If
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Relationships, MediaTypes.Feed, Nothing, Nothing))
         If repositoryInfo.Capabilities.CapabilityGetFolderTree Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.FolderTreeUri(ServiceURIs.enumFolderTreeUri.folderId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "folderId", objId)),
                                      LinkRelationshipTypes.FolderTree, MediaTypes.Tree, Nothing, Nothing))
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Returns links for a cmisfolder in System.ServiceModel.Syndication.SyndicationLink-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="parentId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetFolderLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                     parentId As String) As List(Of AtomPub.AtomLink)
         Return GetFolderLinks(Of AtomPub.AtomLink)(baseUri, repositoryInfo,
                                                    Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind),
                                                    parentId)
      End Function
      ''' <summary>
      ''' Returns links for a cmisfolder in System.Xml.Linq.XElement-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <param name="parentId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetFolderLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                     ns As sxl.XNamespace, elementName As String,
                                     parentId As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetFolderLinks(Of sxl.XElement)(baseUri, repositoryInfo,
                                                   AddressOf .CreateXElement,
                                                   parentId)
         End With
      End Function
#End Region

#Region "links for a cmisitem"
      ''' <summary>
      ''' Returns links for a cmisitem
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="elementFactory"></param>
      ''' <returns></returns>
      ''' <remarks>Used Link Relations:
      ''' 3.11.7 Item Entry; 3.4.3 CMIS Link Relations</remarks>
      Protected Function GetItemLinks(Of TLink)(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                                elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate) As List(Of TLink)
         Dim objId As String = ObjectId
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Relationships, MediaTypes.Feed, Nothing, Nothing))

         Return retVal
      End Function

      ''' <summary>
      ''' Returns links for a cmisitem in System.ServiceModel.Syndication.SyndicationLink-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetItemLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType) As List(Of AtomPub.AtomLink)
         Return GetItemLinks(Of AtomPub.AtomLink)(baseUri, repositoryInfo,
                                                  Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind))
      End Function

      ''' <summary>
      ''' Returns links for a cmisitem in System.Xml.Linq.XElement-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetItemLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                   ns As sxl.XNamespace, elementName As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetItemLinks(Of sxl.XElement)(baseUri, repositoryInfo,
                                                 AddressOf .CreateXElement)
         End With
      End Function
#End Region

#Region "links for a cmispolicy"
      ''' <summary>
      ''' Returns links for a cmispolicy
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="elementFactory"></param>
      ''' <returns></returns>
      ''' <remarks>Used Link Relations:
      ''' 3.11.6 Policy Entry; 3.4.3 CMIS Link Relations</remarks>
      Protected Function GetPolicyLinks(Of TLink)(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                                  elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate) As List(Of TLink)
         Dim objId As String = ObjectId
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         If _renditions IsNot Nothing AndAlso _renditions.Count > 0 Then
            For Each rendition As cmisRenditionType In _renditions
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                         LinkRelationshipTypes.Alternate, rendition.Mimetype, Nothing, rendition.Kind))
            Next
         End If
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objId)),
                                   LinkRelationshipTypes.Relationships, MediaTypes.Feed, Nothing, Nothing))

         Return retVal
      End Function

      ''' <summary>
      ''' Returns links for a cmispolicy in System.ServiceModel.Syndication.SyndicationLink-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetPolicyLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType) As List(Of AtomPub.AtomLink)
         Return GetPolicyLinks(Of AtomPub.AtomLink)(baseUri, repositoryInfo,
                                                    Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind))
      End Function

      ''' <summary>
      ''' Returns links for a cmispolicy in System.Xml.Linq.XElement-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetPolicyLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                     ns As sxl.XNamespace, elementName As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetPolicyLinks(Of sxl.XElement)(baseUri, repositoryInfo,
                                                   AddressOf .CreateXElement)
         End With
      End Function
#End Region

#Region "links for a cmisrelationship"
      ''' <summary>
      ''' Returns links for a cmisrelationship
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="elementFactory"></param>
      ''' <param name="sourceId"></param>
      ''' <param name="targetId"></param>
      ''' <returns></returns>
      ''' <remarks>Used Link Relations:
      ''' 3.11.5 Relationship Entry; 3.4.3 CMIS Link Relations</remarks>
      Protected Function GetRelationshipLinks(Of TLink)(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                                        elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate,
                                                        sourceId As String, targetId As String) As List(Of TLink)
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", ObjectId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", sourceId)),
                                   LinkRelationshipTypes.Source, MediaTypes.Entry, sourceId, Nothing))
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", targetId)),
                                   LinkRelationshipTypes.Target, MediaTypes.Entry, targetId, Nothing))

         Return retVal
      End Function

      ''' <summary>
      ''' Returns links for a cmisrelationship in System.ServiceModel.Syndication.SyndicationLink-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="sourceId"></param>
      ''' <param name="targetId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetRelationshipLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                           sourceId As String, targetId As String) As List(Of AtomPub.AtomLink)
         Return GetRelationshipLinks(Of AtomPub.AtomLink)(baseUri, repositoryInfo,
                                                          Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind),
                                                          sourceId, targetId)
      End Function

      ''' <summary>
      ''' Returns links for a cmisrelationship in System.Xml.Linq.XElement-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <param name="sourceId"></param>
      ''' <param name="targetId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetRelationshipLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                           ns As sxl.XNamespace, elementName As String,
                                           sourceId As String, targetId As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetRelationshipLinks(Of sxl.XElement)(baseUri, repositoryInfo,
                                                         AddressOf .CreateXElement,
                                                         sourceId, targetId)
         End With
      End Function
#End Region

#Region "links for a cmissecondary"
      ''' <summary>
      ''' Returns links for a cmissecondary
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="elementFactory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Function GetSecondaryLinks(Of TLink)(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                                     elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate) As List(Of TLink)
         Return GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)
      End Function

      ''' <summary>
      ''' Returns links for a cmisitem in System.ServiceModel.Syndication.SyndicationLink-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetSecondaryLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType) As List(Of AtomPub.AtomLink)
         Return GetSecondaryLinks(Of AtomPub.AtomLink)(baseUri, repositoryInfo,
                                                       Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind))
      End Function

      ''' <summary>
      ''' Returns links for a cmisitem in System.Xml.Linq.XElement-format
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetSecondaryLinks(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                        ns As sxl.XNamespace, elementName As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetSecondaryLinks(Of sxl.XElement)(baseUri, repositoryInfo,
                                                      AddressOf .CreateXElement)
         End With
      End Function
#End Region

      Public ReadOnly Property Comparer(ParamArray propertyNames As String()) As cmisObjectTypeComparer
         Get
            Return New cmisObjectTypeComparer(Me, propertyNames)
         End Get
      End Property

      ''' <summary>
      ''' Returns the objectTypeId followed by the secondaryObjectTypeIds separated by comma
      ''' </summary>
      Public Function GetCompositeObjectTypeId() As String
         Return String.Join(",", GetObjectTypeIds())
      End Function

      ''' <summary>
      ''' Returns as first element the objectTypeId of the current object followed by the secondaryTypeIds if defined
      ''' </summary>
      Public Iterator Function GetObjectTypeIds() As IEnumerable(Of String)
         Dim objectTypeId As String = Me.ObjectTypeId
         Dim secondaryObjectTypeIds As String() = Me.SecondaryObjectTypeIds
         Dim verify As New HashSet(Of String)

         If String.IsNullOrEmpty(objectTypeId) Then
            Yield objectTypeId
         Else
            Dim objectTypeIds As String() = objectTypeId.Split(","c)

            For index As Integer = 0 To objectTypeIds.Length - 1
               objectTypeId = objectTypeIds(index)
               If verify.Add(objectTypeId) Then Yield objectTypeId
            Next
         End If

         'defined secondaryObjectTypes
         If secondaryObjectTypeIds IsNot Nothing Then
            For index As Integer = 0 To secondaryObjectTypeIds.Length - 1
               Dim secondaryObjectTypeId As String = secondaryObjectTypeIds(index)
               If Not String.IsNullOrEmpty(secondaryObjectTypeId) AndAlso verify.Add(secondaryObjectTypeId) Then Yield secondaryObjectTypeId
            Next
         End If
      End Function

      ''' <summary>
      ''' Search for a specified extension type
      ''' </summary>
      ''' <typeparam name="TExtension"></typeparam>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function FindExtension(Of TExtension As Extensions.Extension)() As TExtension
         Return If(_properties Is Nothing, Nothing, _properties.FindExtension(Of TExtension))
      End Function

      ''' <summary>
      ''' Access to properties via index or PropertyDefinitionId
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property PropertiesAsReadOnly As CmisObjectModel.Collections.Generic.ArrayMapper(Of Collections.cmisPropertiesType, Core.Properties.cmisProperty)
         Get
            Return If(_properties Is Nothing, Nothing, _properties.PropertiesAsReadOnly)
         End Get
      End Property

      Private _relationshipsAsReadOnly As New CmisObjectModel.Collections.Generic.ArrayMapper(Of cmisObjectType, cmisObjectType)(Me,
                                                                                                                                 "Relationships", Function() _relationships,
                                                                                                                                 "ObjectId", Function(cmisRelation)
                                                                                                                                                If TypeOf cmisRelation Is ServiceModel.cmisObjectType Then
                                                                                                                                                   Return CType(cmisRelation, ServiceModel.cmisObjectType).ServiceModel.ObjectId
                                                                                                                                                Else
                                                                                                                                                   Return cmisRelation.ObjectId
                                                                                                                                                End If
                                                                                                                                             End Function)
      ''' <summary>
      ''' Access to relationships via index or objectId of the relationship
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property RelationshipsAsReadOnly As CmisObjectModel.Collections.Generic.ArrayMapper(Of cmisObjectType, cmisObjectType)
         Get
            Return _relationshipsAsReadOnly
         End Get
      End Property

      ''' <summary>
      ''' link to the content of a cmisdocument
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetContentLink(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType,
                                     Optional mediaType As String = MediaTypes.Stream) As AtomPub.AtomLink
         Dim uri As New Uri(baseUri, ServiceURIs.ContentUri(ServiceURIs.enumContentUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", ObjectId))
         Return New AtomPub.AtomLink(uri, LinkRelationshipTypes.ContentStream, mediaType)
      End Function

      ''' <summary>
      ''' link to the content of a cmisdocument
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryInfo"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function GetContentLink(baseUri As Uri, repositoryInfo As Core.cmisRepositoryInfoType, objectId As String,
                                            mediaType As String) As AtomPub.AtomLink
         Dim uri As New Uri(baseUri, ServiceURIs.ContentUri(ServiceURIs.enumContentUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", objectId))
         Return New AtomPub.AtomLink(uri, LinkRelationshipTypes.ContentStream, mediaType)
      End Function

      ''' <summary>
      ''' Returns the properties specified by the given propertyDefinitionIds
      ''' </summary>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns></returns>
      ''' <remarks>The given propertyDefinitionIds handled casesensitive, if there is
      ''' none at all, all properties of this instance will be returned</remarks>
      Public Function GetProperties(ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisProperty)
         Return GetProperties(enumKeySyntax.original, propertyDefinitionIds)
      End Function

      ''' <summary>
      ''' Returns the properties specified by the given propertyDefinitionIds
      ''' </summary>
      ''' <param name="ignoreCase">If True each propertyDefinitionId is compared case insensitive</param>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns>Dictionary of all existing properties specified by propertyDefinitionsIds.
      ''' Notice: if ignoreCase is set to True, then the keys of the returned dictionary are lowercase</returns>
      ''' <remarks>If there are no propertyDefinitionIds defined, all properties of this instance will be returned</remarks>
      Public Function GetProperties(ignoreCase As Boolean, ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisProperty)
         Return GetProperties(If(ignoreCase, enumKeySyntax.lowerCase, enumKeySyntax.original), propertyDefinitionIds)
      End Function

      Public Function GetProperties(keySyntax As enumKeySyntax, ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisProperty)
         If _properties IsNot Nothing Then
            Return _properties.GetProperties(keySyntax, propertyDefinitionIds)
         Else
            Return New Dictionary(Of String, Properties.cmisProperty)
         End If
      End Function

      ''' <summary>
      ''' Handles all property changed events to make sure that _id and _typeId are up to date
      ''' </summary>
      ''' <param name="sender"></param>
      ''' <param name="e"></param>
      ''' <remarks></remarks>
      Private Sub xmlSerializable_PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
         ' Nothing todo..
      End Sub

      Public Shared Operator +(arg1 As cmisObjectType, arg2 As Contracts.ICmisClient) As Client.CmisObject.PreStage
         Return New Client.CmisObject.PreStage(arg2, arg1)
      End Operator
      Public Shared Operator +(arg1 As Contracts.ICmisClient, arg2 As cmisObjectType) As Client.CmisObject.PreStage
         Return New Client.CmisObject.PreStage(arg1, arg2)
      End Operator

      Public Shared Narrowing Operator CType(value As ca.AtomEntry) As cmisObjectType
         Return If(value Is Nothing, Nothing, value.Object)
      End Operator

   End Class
End Namespace