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

      Public Sub New(ParamArray properties As Core.Properties.cmisProperty())
         If properties IsNot Nothing AndAlso properties.Length > 0 Then
            _properties = New Core.Collections.cmisPropertiesType(properties)
         End If
         InitClass()
      End Sub

      Protected Overrides Sub InitClass()
         MyBase.InitClass()
         _observedInstances = New Dictionary(Of Serialization.XmlSerializable, HashSet(Of String))
         RefreshObservation()
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
      Private _allowedChildObjectTypeIds As ccg.Nullable(Of String())
      Public Property AllowedChildObjectTypeIds As ccg.Nullable(Of String())
         Get
            Return _allowedChildObjectTypeIds
         End Get
         Set(value As ccg.Nullable(Of String()))
            AllowedChildObjectTypeIds(True) = value
         End Set
      End Property 'AllowedChildObjectTypeIds
      Private WriteOnly Property AllowedChildObjectTypeIds(updateProperty As Boolean) As ccg.Nullable(Of String())
         Set(value As ccg.Nullable(Of String()))
            If Not value.Equals(_allowedChildObjectTypeIds) Then
               Dim oldValue As ccg.Nullable(Of String()) = _allowedChildObjectTypeIds
               _allowedChildObjectTypeIds = value
               OnPropertyChanged("AllowedChildObjectTypeIds", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.AllowedChildObjectTypeIds,
                                                        Function(factory) factory.AllowedChildObjectTypeIds, value)
            End If
         End Set
      End Property
      Private Shared Sub SetAllowedChildObjectTypeIds(instance As cmisObjectType, value As ccg.Nullable(Of String()))
         instance.AllowedChildObjectTypeIds(False) = value
      End Sub

      Private _baseTypeId As ccg.Nullable(Of String)
      Public Property BaseTypeId As ccg.Nullable(Of String)
         Get
            Return _baseTypeId
         End Get
         Set(value As ccg.Nullable(Of String))
            BaseTypeId(True) = value
         End Set
      End Property 'BaseTypeId
      Private WriteOnly Property BaseTypeId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_baseTypeId) Then
               Dim oldValue As ccg.Nullable(Of String) = _baseTypeId
               _baseTypeId = value
               OnPropertyChanged("BaseTypeId", value, oldValue)
               'if objectTypeId isn't set the baseTypeId defines the typeId
               If String.IsNullOrEmpty(_objectTypeId) Then OnPropertyChanged("ObjectTypeId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.BaseTypeId,
                                                        Function(factory) factory.BaseTypeId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetBaseTypeId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.BaseTypeId(False) = value
      End Sub

      Private _changeToken As ccg.Nullable(Of String)
      Public Property ChangeToken As ccg.Nullable(Of String)
         Get
            Return _changeToken
         End Get
         Set(value As ccg.Nullable(Of String))
            ChangeToken(True) = value
         End Set
      End Property 'ChangeToken
      Private WriteOnly Property ChangeToken(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_changeToken) Then
               Dim oldValue As ccg.Nullable(Of String) = _changeToken
               _changeToken = value
               OnPropertyChanged("ChangeToken", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.ChangeToken,
                                                        Function(factory) factory.ChangeToken, value)
            End If
         End Set
      End Property
      Private Shared Sub SetChangeToken(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.ChangeToken(False) = value
      End Sub

      Private _checkinComment As ccg.Nullable(Of String)
      Public Property CheckinComment As ccg.Nullable(Of String)
         Get
            Return _checkinComment
         End Get
         Set(value As ccg.Nullable(Of String))
            CheckinComment(True) = value
         End Set
      End Property 'CheckinComment
      Private WriteOnly Property CheckinComment(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_checkinComment) Then
               Dim oldValue As ccg.Nullable(Of String) = _checkinComment
               _checkinComment = value
               OnPropertyChanged("CheckinComment", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.CheckinComment,
                                                        Function(factory) factory.CheckinComment, value)
            End If
         End Set
      End Property
      Private Shared Sub SetCheckinComment(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.CheckinComment(False) = value
      End Sub

      Private _contentStreamFileName As ccg.Nullable(Of String)
      Public Property ContentStreamFileName As ccg.Nullable(Of String)
         Get
            Return _contentStreamFileName
         End Get
         Set(value As ccg.Nullable(Of String))
            ContentStreamFileName(True) = value
         End Set
      End Property 'ContentStreamFileName
      Private WriteOnly Property ContentStreamFileName(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_contentStreamFileName) Then
               Dim oldValue As ccg.Nullable(Of String) = _contentStreamFileName
               _contentStreamFileName = value
               OnPropertyChanged("ContentStreamFileName", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.ContentStreamFileName,
                                                        Function(factory) factory.ContentStreamFileName, value)
            End If
         End Set
      End Property
      Private Shared Sub SetContentStreamFileName(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.ContentStreamFileName(False) = value
      End Sub

      Private _contentStreamId As ccg.Nullable(Of String)
      Public Property ContentStreamId As ccg.Nullable(Of String)
         Get
            Return _contentStreamId
         End Get
         Set(value As ccg.Nullable(Of String))
            ContentStreamId(True) = value
         End Set
      End Property 'ContentStreamId
      Private WriteOnly Property ContentStreamId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_contentStreamId) Then
               Dim oldValue As ccg.Nullable(Of String) = _contentStreamId
               _contentStreamId = value
               OnPropertyChanged("ContentStreamId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.ContentStreamId,
                                                        Function(factory) factory.ContentStreamId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetContentStreamId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.ContentStreamId(False) = value
      End Sub

      Private _contentStreamLength As xs_Integer?
      Public Property ContentStreamLength As xs_Integer?
         Get
            Return _contentStreamLength
         End Get
         Set(value As xs_Integer?)
            ContentStreamLength(True) = value
         End Set
      End Property 'ContentStreamLength
      Private WriteOnly Property ContentStreamLength(updateProperty As Boolean) As xs_Integer?
         Set(value As xs_Integer?)
            If Not value.Equals(_contentStreamLength) Then
               Dim oldValue As xs_Integer? = _contentStreamLength
               _contentStreamLength = value
               OnPropertyChanged("ContentStreamLength", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.ContentStreamLength,
                                                        Function(factory) factory.ContentStreamLength, value)
            End If
         End Set
      End Property
      Private Shared Sub SetContentStreamLength(instance As cmisObjectType, value As xs_Integer?)
         instance.ContentStreamLength(False) = value
      End Sub

      Private _contentStreamMimeType As ccg.Nullable(Of String)
      Public Property ContentStreamMimeType As ccg.Nullable(Of String)
         Get
            Return _contentStreamMimeType
         End Get
         Set(value As ccg.Nullable(Of String))
            ContentStreamMimeType(True) = value
         End Set
      End Property 'ContentStreamMimeType
      Private WriteOnly Property ContentStreamMimeType(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_contentStreamMimeType) Then
               Dim oldValue As ccg.Nullable(Of String) = _contentStreamMimeType
               _contentStreamMimeType = value
               OnPropertyChanged("ContentStreamMimeType", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.ContentStreamMimeType,
                                                        Function(factory) factory.ContentStreamMimeType, value)
            End If
         End Set
      End Property
      Private Shared Sub SetContentStreamMimeType(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.ContentStreamMimeType(False) = value
      End Sub

      Private _createdBy As ccg.Nullable(Of String)
      Public Property CreatedBy As ccg.Nullable(Of String)
         Get
            Return _createdBy
         End Get
         Set(value As ccg.Nullable(Of String))
            CreatedBy(True) = value
         End Set
      End Property 'CreatedBy
      Private WriteOnly Property CreatedBy(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_createdBy) Then
               Dim oldValue As ccg.Nullable(Of String) = _createdBy
               _createdBy = value
               OnPropertyChanged("CreatedBy", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.CreatedBy,
                                                        Function(factory) factory.CreatedBy, value)
            End If
         End Set
      End Property
      Private Shared Sub SetCreatedBy(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.CreatedBy(False) = value
      End Sub

      Private _creationDate As DateTimeOffset?
      Public Property CreationDate As DateTimeOffset?
         Get
            Return _creationDate
         End Get
         Set(value As DateTimeOffset?)
            CreationDate(True) = value
         End Set
      End Property 'CreationDate
      Private WriteOnly Property CreationDate(updateProperty As Boolean) As DateTimeOffset?
         Set(value As DateTimeOffset?)
            If Not value.Equals(_creationDate) Then
               Dim oldValue As DateTimeOffset? = _creationDate
               _creationDate = value
               OnPropertyChanged("CreationDate", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.CreationDate,
                                                        Function(factory) factory.CreationDate,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetCreationDate(instance As cmisObjectType, value As DateTimeOffset?)
         instance.CreationDate(False) = value
      End Sub

      Private _description As ccg.Nullable(Of String)
      Public Property Description As ccg.Nullable(Of String)
         Get
            Return _description
         End Get
         Set(value As ccg.Nullable(Of String))
            Description(True) = value
         End Set
      End Property 'Description
      Private WriteOnly Property Description(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_description) Then
               Dim oldValue As ccg.Nullable(Of String) = _description
               _description = value
               OnPropertyChanged("Description", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.Description,
                                                        Function(factory) factory.Description, value)
            End If
         End Set
      End Property
      Private Shared Sub SetDescription(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.Description(False) = value
      End Sub

      Private _isImmutable As Boolean?
      Public Property IsImmutable As Boolean?
         Get
            Return _isImmutable
         End Get
         Set(value As Boolean?)
            IsImmutable(True) = value
         End Set
      End Property 'IsImmutable
      Private WriteOnly Property IsImmutable(updateProperty As Boolean?) As Boolean?
         Set(value As Boolean?)
            If Not value.Equals(_isImmutable) Then
               Dim oldValue As Boolean? = _isImmutable
               _isImmutable = value
               OnPropertyChanged("IsImmutable", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.IsImmutable,
                                                        Function(factory) factory.IsImmutable,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetIsImmutable(instance As cmisObjectType, value As Boolean?)
         instance.IsImmutable(False) = value
      End Sub

      Private _isLatestMajorVersion As Boolean?
      Public Property IsLatestMajorVersion As Boolean?
         Get
            Return _isLatestMajorVersion
         End Get
         Set(value As Boolean?)
            IsLatestMajorVersion(True) = value
         End Set
      End Property 'IsLatestMajorVersion
      Private WriteOnly Property IsLatestMajorVersion(updateProperty As Boolean?) As Boolean?
         Set(value As Boolean?)
            If Not value.Equals(_isLatestMajorVersion) Then
               Dim oldValue As Boolean? = _isLatestMajorVersion
               _isLatestMajorVersion = value
               OnPropertyChanged("IsLatestMajorVersion", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.IsLatestMajorVersion,
                                                        Function(factory) factory.IsLatestMajorVersion,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetIsLatestMajorVersion(instance As cmisObjectType, value As Boolean?)
         instance.IsLatestMajorVersion(False) = value
      End Sub

      Private _isLatestVersion As Boolean?
      Public Property IsLatestVersion As Boolean?
         Get
            Return _isLatestVersion
         End Get
         Set(value As Boolean?)
            IsLatestVersion(True) = value
         End Set
      End Property 'IsLatestVersion
      Private WriteOnly Property IsLatestVersion(updateProperty As Boolean?) As Boolean?
         Set(value As Boolean?)
            If Not value.Equals(_isLatestVersion) Then
               Dim oldValue As Boolean? = _isLatestVersion
               _isLatestVersion = value
               OnPropertyChanged("IsLatestVersion", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.IsLatestVersion,
                                                        Function(factory) factory.IsLatestVersion,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetIsLatestVersion(instance As cmisObjectType, value As Boolean?)
         instance.IsLatestVersion(False) = value
      End Sub

      Private _isMajorVersion As Boolean?
      Public Property IsMajorVersion As Boolean?
         Get
            Return _isMajorVersion
         End Get
         Set(value As Boolean?)
            IsMajorVersion(True) = value
         End Set
      End Property 'IsMajorVersion
      Private WriteOnly Property IsMajorVersion(updateProperty As Boolean?) As Boolean?
         Set(value As Boolean?)
            If Not value.Equals(_isMajorVersion) Then
               Dim oldValue As Boolean? = _isMajorVersion
               _isMajorVersion = value
               OnPropertyChanged("IsMajorVersion", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.IsMajorVersion,
                                                        Function(factory) factory.IsMajorVersion,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetIsMajorVersion(instance As cmisObjectType, value As Boolean?)
         instance.IsMajorVersion(False) = value
      End Sub

      Private _isPrivateWorkingCopy As Boolean?
      Public Property IsPrivateWorkingCopy As Boolean?
         Get
            Return _isPrivateWorkingCopy
         End Get
         Set(value As Boolean?)
            IsPrivateWorkingCopy(True) = value
         End Set
      End Property 'IsPrivateWorkingCopy
      Private WriteOnly Property IsPrivateWorkingCopy(updateProperty As Boolean?) As Boolean?
         Set(value As Boolean?)
            If Not value.Equals(_isPrivateWorkingCopy) Then
               Dim oldValue As Boolean? = _isPrivateWorkingCopy
               _isPrivateWorkingCopy = value
               OnPropertyChanged("IsPrivateWorkingCopy", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.IsPrivateWorkingCopy,
                                                        Function(factory) factory.IsPrivateWorkingCopy,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetIsPrivateWorkingCopy(instance As cmisObjectType, value As Boolean?)
         instance.IsPrivateWorkingCopy(False) = value
      End Sub

      Private _isVersionSeriesCheckedOut As Boolean?
      Public Property IsVersionSeriesCheckedOut As Boolean?
         Get
            Return _isVersionSeriesCheckedOut
         End Get
         Set(value As Boolean?)
            IsVersionSeriesCheckedOut(True) = value
         End Set
      End Property 'IsVersionSeriesCheckedOut
      Private WriteOnly Property IsVersionSeriesCheckedOut(updateProperty As Boolean?) As Boolean?
         Set(value As Boolean?)
            If Not value.Equals(_isVersionSeriesCheckedOut) Then
               Dim oldValue As Boolean? = _isVersionSeriesCheckedOut
               _isVersionSeriesCheckedOut = value
               OnPropertyChanged("IsVersionSeriesCheckedOut", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut,
                                                        Function(factory) factory.IsVersionSeriesCheckedOut,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetIsVersionSeriesCheckedOut(instance As cmisObjectType, value As Boolean?)
         instance.IsVersionSeriesCheckedOut(False) = value
      End Sub

      Private _lastModificationDate As DateTimeOffset?
      Public Property LastModificationDate As DateTimeOffset?
         Get
            Return _lastModificationDate
         End Get
         Set(value As DateTimeOffset?)
            LastModificationDate(True) = value
         End Set
      End Property 'LastModificationDate
      Private WriteOnly Property LastModificationDate(updateProperty As Boolean) As DateTimeOffset?
         Set(value As DateTimeOffset?)
            If Not value.Equals(_lastModificationDate) Then
               Dim oldValue As DateTimeOffset? = _lastModificationDate
               _lastModificationDate = value
               OnPropertyChanged("LastModificationDate", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.LastModificationDate,
                                                        Function(factory) factory.LastModificationDate,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetLastModificationDate(instance As cmisObjectType, value As DateTimeOffset?)
         instance.LastModificationDate(False) = value
      End Sub

      Private _lastModifiedBy As ccg.Nullable(Of String)
      Public Property LastModifiedBy As ccg.Nullable(Of String)
         Get
            Return _lastModifiedBy
         End Get
         Set(value As ccg.Nullable(Of String))
            LastModifiedBy(True) = value
         End Set
      End Property 'LastModifiedBy
      Private WriteOnly Property LastModifiedBy(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_lastModifiedBy) Then
               Dim oldValue As ccg.Nullable(Of String) = _lastModifiedBy
               _lastModifiedBy = value
               OnPropertyChanged("LastModifiedBy", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.LastModifiedBy,
                                                        Function(factory) factory.LastModifiedBy, value)
            End If
         End Set
      End Property
      Private Shared Sub SetLastModifiedBy(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.LastModifiedBy(False) = value
      End Sub

      Private _name As ccg.Nullable(Of String)
      Public Property Name As ccg.Nullable(Of String)
         Get
            Return _name
         End Get
         Set(value As ccg.Nullable(Of String))
            Name(True) = value
         End Set
      End Property 'Name
      Private WriteOnly Property Name(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_name) Then
               Dim oldValue As ccg.Nullable(Of String) = _name
               _name = value
               OnPropertyChanged("Name", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.Name,
                                                        Function(factory) factory.Name, value)
            End If
         End Set
      End Property
      Private Shared Sub SetName(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.Name(False) = value
      End Sub

      Private _objectId As ccg.Nullable(Of String)
      Public Property ObjectId() As ccg.Nullable(Of String)
         Get
            Return _objectId
         End Get
         Set(value As ccg.Nullable(Of String))
            ObjectId(True) = value
         End Set
      End Property 'ObjectId
      Private WriteOnly Property ObjectId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_objectId) Then
               Dim oldValue As ccg.Nullable(Of String) = _objectId
               _objectId = value
               OnPropertyChanged("ObjectId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.ObjectId,
                                                        Function(factory) factory.ObjectId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetObjectId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.ObjectId(False) = value
      End Sub

      Private _objectTypeId As ccg.Nullable(Of String)
      Public Property ObjectTypeId As ccg.Nullable(Of String)
         Get
            'if objectTypeId isn't set the BaseTypeId defines the typeId
            Return If(_objectTypeId.HasValue, _objectTypeId, _baseTypeId)
         End Get
         Set(value As ccg.Nullable(Of String))
            ObjectTypeId(True) = value
         End Set
      End Property 'ObjectTypeId
      Private WriteOnly Property ObjectTypeId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_objectTypeId) Then
               Dim oldValue As ccg.Nullable(Of String) = _objectTypeId
               _objectTypeId = value
               OnPropertyChanged("ObjectTypeId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.ObjectTypeId,
                                                        Function(factory) factory.ObjectTypeId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetObjectTypeId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.ObjectTypeId(False) = value
      End Sub

      Private _parentId As ccg.Nullable(Of String)
      Public Property ParentId As ccg.Nullable(Of String)
         Get
            Return _parentId
         End Get
         Set(value As ccg.Nullable(Of String))
            ParentId(True) = value
         End Set
      End Property 'ParentId
      Private WriteOnly Property ParentId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_parentId) Then
               Dim oldValue As ccg.Nullable(Of String) = _parentId
               _parentId = value
               OnPropertyChanged("ParentId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.ParentId,
                                                        Function(factory) factory.ParentId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetParentId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.ParentId(False) = value
      End Sub

      Private _path As ccg.Nullable(Of String)
      Public Property Path As ccg.Nullable(Of String)
         Get
            Return _path
         End Get
         Set(value As ccg.Nullable(Of String))
            Path(True) = value
         End Set
      End Property 'Path
      Private WriteOnly Property Path(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_path) Then
               Dim oldValue As ccg.Nullable(Of String) = _path
               _path = value
               OnPropertyChanged("Path", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.Path,
                                                        Function(factory) factory.Path, value)
            End If
         End Set
      End Property
      Private Shared Sub SetPath(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.Path(False) = value
      End Sub

      Private _policyText As ccg.Nullable(Of String)
      Public Property PolicyText As ccg.Nullable(Of String)
         Get
            Return _policyText
         End Get
         Set(value As ccg.Nullable(Of String))
            PolicyText(True) = value
         End Set
      End Property 'PolicyText
      Private WriteOnly Property PolicyText(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_policyText) Then
               Dim oldValue As ccg.Nullable(Of String) = _policyText
               _policyText = value
               OnPropertyChanged("PolicyText", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.PolicyText,
                                                        Function(factory) factory.PolicyText, value)
            End If
         End Set
      End Property
      Private Shared Sub SetPolicyText(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.PolicyText(False) = value
      End Sub

      Private _secondaryObjectTypeIds As ccg.Nullable(Of String())
      Public Property SecondaryObjectTypeIds As ccg.Nullable(Of String())
         Get
            Return _secondaryObjectTypeIds
         End Get
         Set(value As ccg.Nullable(Of String()))
            SecondaryObjectTypeIds(True) = value
         End Set
      End Property 'SecondaryObjectTypeIds
      Private WriteOnly Property SecondaryObjectTypeIds(updateProperty As Boolean) As ccg.Nullable(Of String())
         Set(value As ccg.Nullable(Of String()))
            If Not value.Equals(_secondaryObjectTypeIds) Then
               Dim oldValue As ccg.Nullable(Of String()) = _secondaryObjectTypeIds
               _secondaryObjectTypeIds = value
               OnPropertyChanged("SecondaryObjectTypeIds", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.SecondaryObjectTypeIds,
                                                        Function(factory) factory.SecondaryObjectTypeIds,
                                                        value)
            End If
         End Set
      End Property
      Private Shared Sub SetSecondaryObjectTypeIds(instance As cmisObjectType, value As ccg.Nullable(Of String()))
         instance.SecondaryObjectTypeIds(False) = value
      End Sub

      Private _sourceId As ccg.Nullable(Of String)
      Public Property SourceId As ccg.Nullable(Of String)
         Get
            Return _sourceId
         End Get
         Set(value As ccg.Nullable(Of String))
            SourceId(True) = value
         End Set
      End Property 'SourceId
      Private WriteOnly Property SourceId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_sourceId) Then
               Dim oldValue As ccg.Nullable(Of String) = _sourceId
               _sourceId = value
               OnPropertyChanged("SourceId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.SourceId,
                                                        Function(factory) factory.SourceId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetSourceId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.SourceId(False) = value
      End Sub

      Private _targetId As ccg.Nullable(Of String)
      Public Property TargetId As ccg.Nullable(Of String)
         Get
            Return _targetId
         End Get
         Set(value As ccg.Nullable(Of String))
            TargetId(True) = value
         End Set
      End Property 'TargetId
      Private WriteOnly Property TargetId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_targetId) Then
               Dim oldValue As ccg.Nullable(Of String) = _targetId
               _targetId = value
               OnPropertyChanged("TargetId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.TargetId,
                                                        Function(factory) factory.TargetId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetTargetId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.TargetId(False) = value
      End Sub

      Private _versionLabel As ccg.Nullable(Of String)
      Public Property VersionLabel As ccg.Nullable(Of String)
         Get
            Return _versionLabel
         End Get
         Set(value As ccg.Nullable(Of String))
            VersionLabel(True) = value
         End Set
      End Property 'VersionLabel
      Private WriteOnly Property VersionLabel(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_versionLabel) Then
               Dim oldValue As ccg.Nullable(Of String) = _versionLabel
               _versionLabel = value
               OnPropertyChanged("VersionLabel", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.VersionLabel,
                                                        Function(factory) factory.VersionLabel, value)
            End If
         End Set
      End Property
      Private Shared Sub SetVersionLabel(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.VersionLabel(False) = value
      End Sub

      Private _versionSeriesCheckedOutBy As ccg.Nullable(Of String)
      Public Property VersionSeriesCheckedOutBy As ccg.Nullable(Of String)
         Get
            Return _versionSeriesCheckedOutBy
         End Get
         Set(value As ccg.Nullable(Of String))
            VersionSeriesCheckedOutBy(True) = value
         End Set
      End Property 'VersionSeriesCheckedOutBy
      Private WriteOnly Property VersionSeriesCheckedOutBy(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_versionSeriesCheckedOutBy) Then
               Dim oldValue As ccg.Nullable(Of String) = _versionSeriesCheckedOutBy
               _versionSeriesCheckedOutBy = value
               OnPropertyChanged("VersionSeriesCheckedOutBy", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy,
                                                        Function(factory) factory.VersionSeriesCheckedOutBy, value)
            End If
         End Set
      End Property
      Private Shared Sub SetVersionSeriesCheckedOutBy(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.VersionSeriesCheckedOutBy(False) = value
      End Sub

      Private _versionSeriesCheckedOutId As ccg.Nullable(Of String)
      Public Property VersionSeriesCheckedOutId As ccg.Nullable(Of String)
         Get
            Return _versionSeriesCheckedOutId
         End Get
         Set(value As ccg.Nullable(Of String))
            VersionSeriesCheckedOutId(True) = value
         End Set
      End Property 'VersionSeriesCheckedOutId
      Private WriteOnly Property VersionSeriesCheckedOutId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_versionSeriesCheckedOutId) Then
               Dim oldValue As ccg.Nullable(Of String) = _versionSeriesCheckedOutId
               _versionSeriesCheckedOutId = value
               OnPropertyChanged("VersionSeriesCheckedOutId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.VersionSeriesCheckedOutId,
                                                        Function(factory) factory.VersionSeriesCheckedOutId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetVersionSeriesCheckedOutId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.VersionSeriesCheckedOutId(False) = value
      End Sub

      Private _versionSeriesId As ccg.Nullable(Of String)
      Public Property VersionSeriesId As ccg.Nullable(Of String)
         Get
            Return _versionSeriesId
         End Get
         Set(value As ccg.Nullable(Of String))
            VersionSeriesId(True) = value
         End Set
      End Property 'VersionSeriesId
      Private WriteOnly Property VersionSeriesId(updateProperty As Boolean) As ccg.Nullable(Of String)
         Set(value As ccg.Nullable(Of String))
            If Not value.Equals(_versionSeriesId) Then
               Dim oldValue As ccg.Nullable(Of String) = _versionSeriesId
               _versionSeriesId = value
               OnPropertyChanged("VersionSeriesId", value, oldValue)
               If updateProperty Then Me.UpdateProperty(CmisPredefinedPropertyNames.VersionSeriesId,
                                                        Function(factory) factory.VersionSeriesId, value)
            End If
         End Set
      End Property
      Private Shared Sub SetVersionSeriesId(instance As cmisObjectType, value As ccg.Nullable(Of String))
         instance.VersionSeriesId(False) = value
      End Sub

#Region "setter"
      ''' <summary>
      ''' Contains all predefined boolean properties
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _booleanPropertySetter As New Dictionary(Of String, Action(Of cmisObjectType, Boolean)) From {
         {CmisPredefinedPropertyNames.IsImmutable.ToLowerInvariant(), AddressOf SetIsImmutable},
         {CmisPredefinedPropertyNames.IsLatestMajorVersion.ToLowerInvariant(), AddressOf SetIsLatestMajorVersion},
         {CmisPredefinedPropertyNames.IsLatestVersion.ToLowerInvariant(), AddressOf SetIsLatestVersion},
         {CmisPredefinedPropertyNames.IsMajorVersion.ToLowerInvariant(), AddressOf SetIsMajorVersion},
         {CmisPredefinedPropertyNames.IsPrivateWorkingCopy.ToLowerInvariant(), AddressOf SetIsPrivateWorkingCopy},
         {CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut.ToLowerInvariant(), AddressOf SetIsVersionSeriesCheckedOut}}

      ''' <summary>
      ''' Contains all predefined date properties
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _datePropertySetter As New Dictionary(Of String, Action(Of cmisObjectType, DateTimeOffset)) From {
         {CmisPredefinedPropertyNames.CreationDate.ToLowerInvariant(), AddressOf SetCreationDate},
         {CmisPredefinedPropertyNames.LastModificationDate.ToLowerInvariant(), AddressOf SetLastModificationDate}}

      ''' <summary>
      ''' Contains all predefined xs:integer properties
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _xs_integerSetter As New Dictionary(Of String, Action(Of cmisObjectType, xs_Integer)) From {
         {CmisPredefinedPropertyNames.ContentStreamLength.ToLowerInvariant(), AddressOf SetContentStreamLength}}

      ''' <summary>
      ''' Contains all predefined string-array properties
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _stringArraySetter As New Dictionary(Of String, Action(Of cmisObjectType, String())) From {
         {CmisPredefinedPropertyNames.AllowedChildObjectTypeIds.ToLowerInvariant(), AddressOf SetAllowedChildObjectTypeIds},
         {CmisPredefinedPropertyNames.SecondaryObjectTypeIds.ToLowerInvariant(), AddressOf SetSecondaryObjectTypeIds}}

      ''' <summary>
      ''' Contains all predefined string properties
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _stringSetter As New Dictionary(Of String, Action(Of cmisObjectType, String)) From {
         {CmisPredefinedPropertyNames.BaseTypeId.ToLowerInvariant(), AddressOf SetBaseTypeId},
         {CmisPredefinedPropertyNames.ChangeToken.ToLowerInvariant(), AddressOf SetChangeToken},
         {CmisPredefinedPropertyNames.CheckinComment.ToLowerInvariant(), AddressOf SetCheckinComment},
         {CmisPredefinedPropertyNames.ContentStreamFileName.ToLowerInvariant(), AddressOf SetContentStreamFileName},
         {CmisPredefinedPropertyNames.ContentStreamId.ToLowerInvariant(), AddressOf SetContentStreamId},
         {CmisPredefinedPropertyNames.ContentStreamMimeType.ToLowerInvariant(), AddressOf SetContentStreamMimeType},
         {CmisPredefinedPropertyNames.CreatedBy.ToLowerInvariant(), AddressOf SetCreatedBy},
         {CmisPredefinedPropertyNames.Description.ToLowerInvariant(), AddressOf SetDescription},
         {CmisPredefinedPropertyNames.LastModifiedBy.ToLowerInvariant(), AddressOf SetLastModifiedBy},
         {CmisPredefinedPropertyNames.Name.ToLowerInvariant(), AddressOf SetName},
         {CmisPredefinedPropertyNames.ObjectId.ToLowerInvariant(), AddressOf SetObjectId},
         {CmisPredefinedPropertyNames.ObjectTypeId.ToLowerInvariant(), AddressOf SetObjectTypeId},
         {CmisPredefinedPropertyNames.ParentId.ToLowerInvariant(), AddressOf SetParentId},
         {CmisPredefinedPropertyNames.Path.ToLowerInvariant(), AddressOf SetPath},
         {CmisPredefinedPropertyNames.PolicyText.ToLowerInvariant(), AddressOf SetPolicyText},
         {CmisPredefinedPropertyNames.SourceId.ToLowerInvariant(), AddressOf SetSourceId},
         {CmisPredefinedPropertyNames.TargetId.ToLowerInvariant(), AddressOf SetTargetId},
         {CmisPredefinedPropertyNames.VersionLabel.ToLowerInvariant(), AddressOf SetVersionLabel},
         {CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy.ToLowerInvariant(), AddressOf SetVersionSeriesCheckedOutBy},
         {CmisPredefinedPropertyNames.VersionSeriesCheckedOutId.ToLowerInvariant(), AddressOf SetVersionSeriesCheckedOutId},
         {CmisPredefinedPropertyNames.VersionSeriesId.ToLowerInvariant(), AddressOf SetVersionSeriesId}}
#End Region

#End Region

#Region "IXmlSerializable"
      Public Overrides Sub ReadXml(reader As System.Xml.XmlReader)
         MyBase.ReadXml(reader)
         RefreshObservation()
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

         Dim retVal As New List(Of TLink) From {
            elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryId, "id", _objectId)),
                           LinkRelationshipTypes.Self, MediaTypes.Entry, _objectId, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.GetRepositoryInfo.ReplaceUri("repositoryId", repositoryId)),
                           LinkRelationshipTypes.Service, MediaTypes.Service, Nothing, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", repositoryId, "id", _objectTypeId)),
                           LinkRelationshipTypes.DescribedBy, MediaTypes.Entry, Nothing, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.GetAllowableActions.ReplaceUri("repositoryId", repositoryId, "id", _objectId)),
                           LinkRelationshipTypes.AllowableActions, MediaTypes.AllowableActions, Nothing, Nothing)}

         If repositoryInfo.Capabilities.CapabilityACL <> enumCapabilityACL.none Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.GetAcl.ReplaceUri("repositoryId", repositoryId, "id", _objectId)),
                       LinkRelationshipTypes.Acl, MediaTypes.Acl, Nothing, Nothing))
         End If
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.PoliciesUri(ServiceURIs.enumPoliciesUri.objectId).ReplaceUri("repositoryId", repositoryId, "id", _objectId)),
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
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)
         Dim isWorkingCopy As Boolean = Not (String.IsNullOrEmpty(privateWorkingCopyId) OrElse _objectId <> privateWorkingCopyId)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectParentsUri(ServiceURIs.enumObjectParentsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                   LinkRelationshipTypes.Up, MediaTypes.Feed, Nothing, Nothing))
         If canGetAllVersions Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.AllVersionsUri(ServiceURIs.enumAllVersionsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                      LinkRelationshipTypes.VersionHistory, MediaTypes.Feed, Nothing, Nothing))
         End If
         If Not (isLatestVersion OrElse isWorkingCopy) Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.specifyVersion).ReplaceUri("returnVersion", GetName(RestAtom.enumReturnVersion.latest), "repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                      LinkRelationshipTypes.CurrentVersion, MediaTypes.Entry, Nothing, Nothing))
         End If
         If repositoryInfo.Capabilities.CapabilityContentStreamUpdatability <> Core.enumCapabilityContentStreamUpdates.none Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ContentUri(ServiceURIs.enumContentUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                      LinkRelationshipTypes.EditMedia, MediaTypes.Stream, Nothing, Nothing))
         End If
         If Not String.IsNullOrEmpty(privateWorkingCopyId) Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.workingCopy).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", privateWorkingCopyId, "pwc", "true")),
                                      LinkRelationshipTypes.WorkingCopy, MediaTypes.Entry, privateWorkingCopyId, Nothing))
         End If
         If _renditions IsNot Nothing AndAlso _renditions.Count > 0 Then
            For Each rendition As cmisRenditionType In _renditions
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ContentUri(ServiceURIs.enumContentUri.getContentStream).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId, "streamId", rendition.StreamId)),
                                         LinkRelationshipTypes.Alternate, rendition.Mimetype, Nothing, rendition.Kind))
            Next
         End If
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
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
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ChildrenUri(ServiceURIs.enumChildrenUri.folderId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                   LinkRelationshipTypes.Down, MediaTypes.Feed, Nothing, Nothing))
         If repositoryInfo.Capabilities.CapabilityGetDescendants Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.DescendantsUri(ServiceURIs.enumDescendantsUri.folderId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                      LinkRelationshipTypes.Down, MediaTypes.Tree, Nothing, Nothing))
         End If
         If Not String.IsNullOrEmpty(parentId) Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", parentId)),
                                      LinkRelationshipTypes.Up, MediaTypes.Entry, parentId, Nothing))
         End If
         If _renditions IsNot Nothing AndAlso _renditions.Count > 0 Then
            For Each rendition As cmisRenditionType In _renditions
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                         LinkRelationshipTypes.Alternate, rendition.Mimetype, Nothing, rendition.Kind))
            Next
         End If
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                   LinkRelationshipTypes.Relationships, MediaTypes.Feed, Nothing, Nothing))
         If repositoryInfo.Capabilities.CapabilityGetFolderTree Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.FolderTreeUri(ServiceURIs.enumFolderTreeUri.folderId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "folderId", _objectId)),
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
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
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
         Dim retVal As List(Of TLink) = GetLinks(Of TLink)(baseUri, repositoryInfo, elementFactory)

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                   LinkRelationshipTypes.Edit, MediaTypes.Entry, Nothing, Nothing))
         If _renditions IsNot Nothing AndAlso _renditions.Count > 0 Then
            For Each rendition As cmisRenditionType In _renditions
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
                                         LinkRelationshipTypes.Alternate, rendition.Mimetype, Nothing, rendition.Kind))
            Next
         End If
         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
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

         retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.self).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId)),
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
         Dim uri As New Uri(baseUri, ServiceURIs.ContentUri(ServiceURIs.enumContentUri.objectId).ReplaceUri("repositoryId", repositoryInfo.RepositoryId, "id", _objectId))
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

      Private _observedInstances As Dictionary(Of Serialization.XmlSerializable, HashSet(Of String))

      ''' <summary>
      ''' Handles all property changed events to make sure that _id and _typeId are up to date
      ''' </summary>
      ''' <param name="sender"></param>
      ''' <param name="e"></param>
      ''' <remarks></remarks>
      Private Sub xmlSerializable_PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
         If TypeOf sender Is Serialization.XmlSerializable AndAlso e IsNot Nothing Then
            Dim instance As Serialization.XmlSerializable = CType(sender, Serialization.XmlSerializable)

            If _observedInstances.ContainsKey(instance) AndAlso _observedInstances(instance).Contains(e.PropertyName) Then
               RefreshObservation()
            End If
         End If
      End Sub

      ''' <summary>
      ''' Updates the necessary propertychanged-handler
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub RefreshObservation()
         'remove handler from current
         For Each instance As Serialization.XmlSerializable In _observedInstances.Keys
            If TypeOf instance Is Core.Properties.cmisProperty Then
               RemoveHandler CType(instance, Core.Properties.cmisProperty).ExtendedPropertyChanged, AddressOf xmlSerializable_PropertyChanged
            Else
               RemoveHandler instance.PropertyChanged, AddressOf xmlSerializable_PropertyChanged
            End If
         Next
         _observedInstances.Clear()

         _observedInstances.Add(Me, New HashSet(Of String) From {"Properties"})
         AddHandler PropertyChanged, AddressOf xmlSerializable_PropertyChanged
         If _properties Is Nothing Then
            AllowedChildObjectTypeIds = Nothing
            BaseTypeId = Nothing
            ChangeToken = Nothing
            CheckinComment = Nothing
            ContentStreamFileName = Nothing
            ContentStreamId = Nothing
            ContentStreamLength = Nothing
            ContentStreamMimeType = Nothing
            CreatedBy = Nothing
            CreationDate = Nothing
            Description = Nothing
            IsImmutable = Nothing
            IsLatestMajorVersion = Nothing
            IsLatestVersion = Nothing
            IsMajorVersion = Nothing
            IsPrivateWorkingCopy = Nothing
            IsVersionSeriesCheckedOut = Nothing
            LastModificationDate = Nothing
            LastModifiedBy = Nothing
            Name = Nothing
            ObjectId = Nothing
            ObjectTypeId = Nothing
            ParentId = Nothing
            Path = Nothing
            PolicyText = Nothing
            SecondaryObjectTypeIds = Nothing
            SourceId = Nothing
            TargetId = Nothing
            VersionLabel = Nothing
            VersionSeriesCheckedOutBy = Nothing
            VersionSeriesCheckedOutId = Nothing
            VersionSeriesId = Nothing
         Else
            _observedInstances.Add(_properties, New HashSet(Of String) From {"Properties"})
            AddHandler _properties.PropertyChanged, AddressOf xmlSerializable_PropertyChanged

            'get predefined properties from the property-collection
            Dim properties As Dictionary(Of String, Properties.cmisProperty) =
               GetProperties(True, CmisPredefinedPropertyNames.AllowedChildObjectTypeIds,
                             CmisPredefinedPropertyNames.BaseTypeId, CmisPredefinedPropertyNames.ChangeToken,
                             CmisPredefinedPropertyNames.CheckinComment, CmisPredefinedPropertyNames.ContentStreamFileName,
                             CmisPredefinedPropertyNames.ContentStreamId, CmisPredefinedPropertyNames.ContentStreamLength,
                             CmisPredefinedPropertyNames.ContentStreamMimeType, CmisPredefinedPropertyNames.CreatedBy,
                             CmisPredefinedPropertyNames.CreationDate, CmisPredefinedPropertyNames.Description,
                             CmisPredefinedPropertyNames.IsImmutable,
                             CmisPredefinedPropertyNames.IsLatestMajorVersion, CmisPredefinedPropertyNames.IsLatestVersion,
                             CmisPredefinedPropertyNames.IsMajorVersion, CmisPredefinedPropertyNames.IsPrivateWorkingCopy,
                             CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut, CmisPredefinedPropertyNames.LastModificationDate,
                             CmisPredefinedPropertyNames.LastModifiedBy, CmisPredefinedPropertyNames.Name,
                             CmisPredefinedPropertyNames.ObjectId, CmisPredefinedPropertyNames.ObjectTypeId,
                             CmisPredefinedPropertyNames.ParentId, CmisPredefinedPropertyNames.Path,
                             CmisPredefinedPropertyNames.PolicyText, CmisPredefinedPropertyNames.SecondaryObjectTypeIds,
                             CmisPredefinedPropertyNames.SourceId, CmisPredefinedPropertyNames.TargetId,
                             CmisPredefinedPropertyNames.VersionLabel, CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy,
                             CmisPredefinedPropertyNames.VersionSeriesCheckedOutId, CmisPredefinedPropertyNames.VersionSeriesId)
            'predefined boolean properties
            For Each de As KeyValuePair(Of String, Action(Of cmisObjectType, Boolean)) In _booleanPropertySetter
               If properties.ContainsKey(de.Key) Then
                  Dim cmisProperty As Core.Properties.cmisProperty = properties(de.Key)
                  Try
                     de.Value.Invoke(Me, CBool(cmisProperty.Value))
                  Catch
                  End Try
                  _observedInstances.Add(cmisProperty, New HashSet(Of String) From {"PropertyDefinitionId", "Values"})
                  AddHandler cmisProperty.ExtendedPropertyChanged, AddressOf xmlSerializable_PropertyChanged
               Else
                  de.Value.Invoke(Me, False)
               End If
            Next
            'predefined date properties
            For Each de As KeyValuePair(Of String, Action(Of cmisObjectType, DateTimeOffset)) In _datePropertySetter
               If properties.ContainsKey(de.Key) Then
                  Dim cmisProperty As Core.Properties.cmisProperty = properties(de.Key)
                  'maybe the chosen transfer used succinct in browser-binding, then dateTime-properties are interpreted as
                  'integer-properties
                  If TypeOf cmisProperty Is Properties.cmisPropertyInteger Then
                     Dim dateTimeProperty As Properties.cmisPropertyDateTime =
                        DirectCast(cmisProperty, Properties.cmisPropertyInteger).ToDateTimeProperty()
                     Dim allProperties As Properties.cmisProperty() = _properties.Properties
                     'allProperties is a valid instance otherwise this codesection have not been reached
                     For index As Integer = 0 To allProperties.Length - 1
                        If allProperties(index) Is cmisProperty Then
                           allProperties(index) = dateTimeProperty
                           Exit For
                        End If
                     Next
                     cmisProperty = dateTimeProperty
                  End If
                  Try
                     de.Value.Invoke(Me, CType(cmisProperty.Value, DateTimeOffset))
                  Catch
                  End Try
                  _observedInstances.Add(cmisProperty, New HashSet(Of String) From {"PropertyDefinitionId", "Values"})
                  AddHandler cmisProperty.ExtendedPropertyChanged, AddressOf xmlSerializable_PropertyChanged
               Else
                  de.Value.Invoke(Me, DateTimeOffset.MinValue)
               End If
            Next
            'predefined xs:integer properties
            For Each de As KeyValuePair(Of String, Action(Of cmisObjectType, xs_Integer)) In _xs_integerSetter
               If properties.ContainsKey(de.Key) Then
                  Dim cmisProperty As Core.Properties.cmisProperty = properties(de.Key)
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
                  Try
                     de.Value.Invoke(Me, CInt(cmisProperty.Value))
                  Catch
                  End Try
#Else
                  Try
                     de.Value.Invoke(Me, CLng(cmisProperty.Value))
                  Catch
                  End Try
#End If
                  _observedInstances.Add(cmisProperty, New HashSet(Of String) From {"PropertyDefinitionId", "Values"})
                  AddHandler cmisProperty.ExtendedPropertyChanged, AddressOf xmlSerializable_PropertyChanged
               Else
                  de.Value.Invoke(Me, 0)
               End If
            Next
            'predefined string properties
            For Each de As KeyValuePair(Of String, Action(Of cmisObjectType, String)) In _stringSetter
               If properties.ContainsKey(de.Key) Then
                  Dim cmisProperty As Core.Properties.cmisProperty = properties(de.Key)
                  Try
                     de.Value.Invoke(Me, CStr(cmisProperty.Value))
                  Catch
                  End Try
                  _observedInstances.Add(cmisProperty, New HashSet(Of String) From {"PropertyDefinitionId", "Values"})
                  AddHandler cmisProperty.ExtendedPropertyChanged, AddressOf xmlSerializable_PropertyChanged
               Else
                  de.Value.Invoke(Me, Nothing)
               End If
            Next
            'predefined string-array properties
            For Each de As KeyValuePair(Of String, Action(Of cmisObjectType, String())) In _stringArraySetter
               If properties.ContainsKey(de.Key) Then
                  Dim cmisProperty As Core.Properties.cmisProperty = properties(de.Key)
                  Try
                     de.Value.Invoke(Me, CType(cmisProperty, Core.Properties.Generic.cmisProperty(Of String)).Values)
                  Catch
                  End Try
                  _observedInstances.Add(cmisProperty, New HashSet(Of String) From {"PropertyDefinitionId", "Values"})
                  AddHandler cmisProperty.ExtendedPropertyChanged, AddressOf xmlSerializable_PropertyChanged
               Else
                  de.Value.Invoke(Me, Nothing)
               End If
            Next
         End If
      End Sub

      ''' <summary>
      ''' Sets nullable.Value to the specified cmisProperty if nullable.HasValue is True otherwise set cmisProperty to notSet
      ''' </summary>
      ''' <remarks>Boolean? overload</remarks>
      Private Sub UpdateProperty(propertyDefinitionId As String,
                                 getPropertyDefinition As Func(Of Common.PredefinedPropertyDefinitionFactory, Core.Definitions.Properties.cmisPropertyDefinitionType),
                                 nullable As Boolean?)
         If nullable.HasValue Then
            UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition, nullable.Value)
         Else
            UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition)
         End If
      End Sub
      ''' <summary>
      ''' Sets nullable.Value to the specified cmisProperty if nullable.HasValue is True otherwise set cmisProperty to notSet
      ''' </summary>
      ''' <remarks>DateTimeOffset? overload</remarks>
      Private Sub UpdateProperty(propertyDefinitionId As String,
                                 getPropertyDefinition As Func(Of Common.PredefinedPropertyDefinitionFactory, Core.Definitions.Properties.cmisPropertyDefinitionType),
                                 nullable As DateTimeOffset?)
         If nullable.HasValue Then
            UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition, nullable.Value)
         Else
            UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition)
         End If
      End Sub
      ''' <summary>
      ''' Sets nullable.Value to the specified cmisProperty if nullable.HasValue is True otherwise set cmisProperty to notSet
      ''' </summary>
      ''' <remarks>xs_Integer? overload</remarks>
      Private Sub UpdateProperty(propertyDefinitionId As String,
                                 getPropertyDefinition As Func(Of Common.PredefinedPropertyDefinitionFactory, Core.Definitions.Properties.cmisPropertyDefinitionType),
                                 nullable As xs_Integer?)
         If nullable.HasValue Then
            UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition, nullable.Value)
         Else
            UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition)
         End If
      End Sub
      ''' <summary>
      ''' Sets nullable.Value to the specified cmisProperty if nullable.HasValue is True or removes the cmisProperty
      ''' </summary>
      ''' <remarks>Nullable(Of TValue) overload</remarks>
      Private Sub UpdateProperty(Of TValue)(propertyDefinitionId As String,
                                            getPropertyDefinition As Func(Of Common.PredefinedPropertyDefinitionFactory, Core.Definitions.Properties.cmisPropertyDefinitionType),
                                            nullable As ccg.Nullable(Of TValue))
         If nullable.HasValue Then
            Dim value As TValue = nullable.Value

            If value Is Nothing Then
               UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition)
            ElseIf GetType(TValue).IsArray Then
               UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition, (From item As Object In CType(CObj(value), Array)
                                                                                Select item).ToArray())
            Else
               UpdatePropertyCore(propertyDefinitionId, getPropertyDefinition, value)
            End If
         ElseIf _properties IsNot Nothing Then
            _properties.RemoveProperty(propertyDefinitionId)
         End If
      End Sub
      ''' <summary>
      ''' Sets values to the specified cmisProperty
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub UpdatePropertyCore(propertyDefinitionId As String,
                                     getPropertyDefinition As Func(Of Common.PredefinedPropertyDefinitionFactory, Core.Definitions.Properties.cmisPropertyDefinitionType),
                                     ParamArray values As Object())
         If _properties Is Nothing Then
            Properties = New Core.Collections.cmisPropertiesType(getPropertyDefinition(New Common.PredefinedPropertyDefinitionFactory(Nothing)).CreateProperty(values))
         Else
            With GetProperties(True, propertyDefinitionId)
               If .Count = 1 Then
                  'only the Value-property of the cmisProperty has to be updated
                  .Item(propertyDefinitionId.ToLowerInvariant()).Values = values
               Else
                  'new cmisProperty has to be created
                  Dim properties As New List(Of Core.Properties.cmisProperty)

                  If _properties.Properties IsNot Nothing Then properties.AddRange(_properties.Properties)
                  properties.Add(getPropertyDefinition(New Common.PredefinedPropertyDefinitionFactory(Nothing)).CreateProperty(values))
                  _properties.Properties = properties.ToArray()
               End If
            End With
         End If
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