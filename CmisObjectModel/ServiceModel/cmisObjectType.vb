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
Imports CmisObjectModel.Core
Imports ca = CmisObjectModel.AtomPub
Imports sss = System.ServiceModel.Syndication

Namespace CmisObjectModel.ServiceModel
   Public Class cmisObjectType
      Inherits Core.cmisObjectType
      Implements Contracts.IServiceModelObject

      <Obsolete("Constructor defined for serialization usage only", True),
       System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Public Sub New()
         MyBase.New()
         Me._serviceModel = New ServiceModelExtension(Me)
      End Sub
      Public Sub New(serviceModel As ServiceModelExtension,
                     ParamArray properties As Core.Properties.cmisProperty())
         MyBase.New(properties)
         Me._serviceModel = If(serviceModel, New ServiceModelExtension(Me))
      End Sub

#Region "Helper classes"
      ''' <summary>
      ''' Summary of object properties in a bulkUpdateProperties-call
      ''' </summary>
      ''' <remarks></remarks>
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Public Class BulkUpdatePropertiesExtension

         Friend Sub New(owner As cmisObjectType)
            _owner = owner
         End Sub

         ''' <summary>
         ''' NewId if the bulkUpdateProperties-call modified the objectId
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Property NewId As String
            Get
               Dim newIdExtension As Extensions.Common.NewIdExtension = _owner.FindExtension(Of Extensions.Common.NewIdExtension)()
               Return If(newIdExtension Is Nothing, Nothing, newIdExtension.NewId)
            End Get
            Set(value As String)
               If Not String.IsNullOrEmpty(value) Then
                  Dim newIdExtension As Extensions.Common.NewIdExtension = _owner.FindExtension(Of Extensions.Common.NewIdExtension)()

                  If newIdExtension Is Nothing Then
                     Dim currentExtensions As Extensions.Extension()
                     Dim length As Integer

                     newIdExtension = New Extensions.Common.NewIdExtension() With {.NewId = value}
                     If _owner._properties Is Nothing Then _owner._properties = New Core.Collections.cmisPropertiesType()
                     currentExtensions = _owner._properties.Extensions
                     length = If(currentExtensions Is Nothing, 0, currentExtensions.Length)
                     If length = 0 Then
                        _owner._properties.Extensions = New Extensions.Extension() {newIdExtension}
                     Else
                        Dim newExtensions As Extensions.Extension() = CType(Array.CreateInstance(GetType(Extensions.Extension), length + 1), Extensions.Extension())

                        Array.Copy(currentExtensions, newExtensions, length)
                        newExtensions(length) = newIdExtension
                        _owner._properties.Extensions = newExtensions
                     End If
                  Else
                     newIdExtension.NewId = value
                  End If
               ElseIf _owner._properties IsNot Nothing AndAlso _owner._properties.Extensions IsNot Nothing Then
                  'delete NewIdExtension if existing
                  Dim extensions As Extensions.Extension() = (From extension As Extensions.Extension In _owner._properties.Extensions
                                                              Where Not TypeOf extension Is Extensions.Common.NewIdExtension
                                                              Select extension).ToArray()
                  _owner._properties.Extensions = If(extensions.Length = 0, Nothing, extensions)
               End If
            End Set
         End Property

         Private _owner As cmisObjectType

      End Class

      ''' <summary>
      ''' Summary of the objects properties to allow the system to create AtomPub-Links
      ''' </summary>
      ''' <remarks></remarks>
      Public Class ServiceModelExtension

         Protected Friend Sub New(owner As cmisObjectType)
            _owner = If(owner, _emptyOwner)
         End Sub
         Public Sub New(objectId As String, baseObjectType As Core.enumBaseObjectTypeIds,
                        summary As String, parentId As String,
                        publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                        isLatestVersion As Boolean,
                        versionSeriesId As String, versionSeriesCheckedOutId As String,
                        contentLink As ca.AtomLink,
                        ParamArray authors As sss.SyndicationPerson())
            _authors = authors
            _lastUpdatedTime = lastUpdatedTime
            _publishDate = publishDate
            _baseObjectType = baseObjectType
            Me.ContentLink = contentLink
            _isLatestVersion = isLatestVersion
            _objectId = objectId
            _parentId = parentId
            _summary = summary
            _versionSeriesId = versionSeriesId
            _versionSeriesCheckedOutId = versionSeriesCheckedOutId
            'ensure a valid instance
            _owner = _emptyOwner
         End Sub
         Public Sub New(objectId As String, baseObjectType As Core.enumBaseObjectTypeIds,
                        summary As String, parentId As String,
                        publishDate As Date, lastUpdatedTime As Date,
                        isLatestVersion As Boolean,
                        versionSeriesId As String, versionSeriesCheckedOutId As String,
                        contentLink As ca.AtomLink,
                        ParamArray authors As sss.SyndicationPerson())
            Me.New(objectId, baseObjectType, summary, parentId, ToDateTimeOffset(publishDate), ToDateTimeOffset(lastUpdatedTime),
                   isLatestVersion, versionSeriesId, versionSeriesCheckedOutId, contentLink, authors)
         End Sub
         ''' <summary>
         ''' Relationship-Overload
         ''' </summary>
         ''' <remarks></remarks>
         Public Sub New(objectId As String, sourceId As String, targetId As String,
                        publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                        isLatestVersion As Boolean,
                        versionSeriesId As String, versionSeriesCheckedOutId As String,
                        ParamArray authors As sss.SyndicationPerson())
            Me.New(objectId, Core.enumBaseObjectTypeIds.cmisRelationship, Nothing, Nothing,
                   publishDate, lastUpdatedTime, isLatestVersion,
                   versionSeriesId, versionSeriesCheckedOutId, Nothing, authors)
            Me.SourceId = sourceId
            Me.TargetId = targetId
         End Sub
         Public Sub New(objectId As String, sourceId As String, targetId As String,
                        publishDate As Date, lastUpdatedTime As Date,
                        isLatestVersion As Boolean,
                        versionSeriesId As String, versionSeriesCheckedOutId As String,
                        ParamArray authors As sss.SyndicationPerson())
            Me.New(objectId, sourceId, targetId, ToDateTimeOffset(publishDate), ToDateTimeOffset(lastUpdatedTime),
                   isLatestVersion, versionSeriesId, versionSeriesCheckedOutId, authors)
         End Sub

         ''' <summary>
         ''' Support for systems which provide only one author per cmisObject
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Property Author As sss.SyndicationPerson
            Get
               Return If(_authors Is Nothing OrElse _authors.Length = 0, Nothing, _authors(0))
            End Get
            Set(value As sss.SyndicationPerson)
               If value Is Nothing Then
                  _authors = Nothing
               Else
                  _authors = New sss.SyndicationPerson() {value}
               End If
            End Set
         End Property
         Private _authors As sss.SyndicationPerson()
         Public Property Authors As sss.SyndicationPerson()
            Get
               Return _authors
            End Get
            Set(value As sss.SyndicationPerson())
               _authors = value
            End Set
         End Property

         Private _baseObjectType As enumBaseObjectTypeIds?
         Public Property BaseObjectType As enumBaseObjectTypeIds
            Get
               If _baseObjectType.HasValue Then
                  Return _baseObjectType.Value
               Else
                  Dim baseTypeId As String = _owner.BaseTypeId
                  Dim retVal As enumBaseObjectTypeIds = enumBaseObjectTypeIds.cmisDocument

                  If Not String.IsNullOrEmpty(baseTypeId) Then
                     baseTypeId.TryParse(retVal, True)
                  End If

                  Return retVal
               End If
            End Get
            Set(value As enumBaseObjectTypeIds)
               _baseObjectType = value
            End Set
         End Property

         ''' <summary>
         ''' Reset BaseObjectType
         ''' </summary>
         Public Sub ClearBaseObjectType()
            _baseObjectType = Nothing
         End Sub
         ''' <summary>
         ''' Reset IsLatestVersion
         ''' </summary>
         Public Sub ClearIsLatestVersion()
            _isLatestVersion = Nothing
         End Sub
         ''' <summary>
         ''' Reset LastUpdatedTime
         ''' </summary>
         Public Sub ClearLastUpdatedTime()
            _lastUpdatedTime = Nothing
         End Sub
         ''' <summary>
         ''' Reset ObjectId
         ''' </summary>
         Public Sub ClearObjectId()
            _objectId = Nothing
         End Sub
         ''' <summary>
         ''' Reset ParentId
         ''' </summary>
         Public Sub ClearParentId()
            _parentId = Nothing
         End Sub
         ''' <summary>
         ''' Reset PublishDate
         ''' </summary>
         Public Sub ClearPublishDate()
            _publishDate = Nothing
         End Sub
         ''' <summary>
         ''' Reset SourceId
         ''' </summary>
         Public Sub ClearSourceId()
            _sourceId = Nothing
         End Sub
         ''' <summary>
         ''' Reset Summary
         ''' </summary>
         Public Sub ClearSummary()
            _summary = Nothing
         End Sub
         ''' <summary>
         ''' Reset TargetId
         ''' </summary>
         Public Sub ClearTargetId()
            _targetId = Nothing
         End Sub
         ''' <summary>
         ''' Reset VersionSeriesCheckedOutId
         ''' </summary>
         Public Sub ClearVersionSeriesCheckedOutId()
            _versionSeriesCheckedOutId = Nothing
         End Sub
         ''' <summary>
         ''' Reset VersionSeriesId
         ''' </summary>
         Public Sub ClearVersionSeriesId()
            _versionSeriesId = Nothing
         End Sub

         Public Property ContentLink As ca.AtomLink
         Private Shared _emptyOwner As New cmisObjectType(Nothing)

         Private _isLatestVersion As Boolean?
         Public Property IsLatestVersion As Boolean
            Get
               Return If(_isLatestVersion.HasValue, _isLatestVersion.Value,
                         If(_owner.IsLatestVersion.HasValue, _owner.IsLatestVersion.Value, False))
            End Get
            Set(value As Boolean)
               _isLatestVersion = value
            End Set
         End Property

         Private _lastUpdatedTime As DateTimeOffset?
         Public Property LastUpdatedTime As DateTimeOffset
            Get
               Dim retVal As DateTimeOffset = If(_lastUpdatedTime.HasValue, _lastUpdatedTime.Value,
                                                 If(_owner.LastModificationDate.HasValue, _owner.LastModificationDate.Value, DateTimeOffset.UtcNow))
               Return If(retVal.CompareTo(DateTimeOffset.MinValue) = 0, DateTimeOffset.UtcNow, retVal)
            End Get
            Set(value As DateTimeOffset)
               _lastUpdatedTime = value
            End Set
         End Property

         Private _objectId As Common.Generic.Nullable(Of String)
         Public Property ObjectId As String
            Get
               Return If(_objectId.HasValue, _objectId.Value, _owner.ObjectId.Value)
            End Get
            Set(value As String)
               _objectId = value
            End Set
         End Property

         Private _owner As cmisObjectType

         Private _parentId As Common.Generic.Nullable(Of String)
         Public Property ParentId As String
            Get
               Return If(_parentId.HasValue, _parentId.Value, _owner.ParentId.Value)
            End Get
            Set(value As String)
               _parentId = value
            End Set
         End Property

         Private _publishDate As DateTimeOffset?
         Public Property PublishDate As DateTimeOffset
            Get
               Dim retVal As DateTimeOffset = If(_publishDate.HasValue, _publishDate.Value,
                                                 If(_owner.CreationDate.HasValue, _owner.CreationDate.Value, DateTimeOffset.UtcNow))
               Return If(retVal.CompareTo(DateTimeOffset.MinValue) = 0, DateTimeOffset.UtcNow, retVal)
            End Get
            Set(value As DateTimeOffset)
               _publishDate = value
            End Set
         End Property

         Private _sourceId As Common.Generic.Nullable(Of String)
         Public Property SourceId As String
            Get
               Return If(_sourceId.HasValue, _sourceId.Value, _owner.SourceId.Value)
            End Get
            Set(value As String)
               _sourceId = value
            End Set
         End Property

         Private _summary As Common.Generic.Nullable(Of String)
         Public Property Summary As String
            Get
               Return If(_summary.HasValue, _summary.Value, _owner.Description.Value)
            End Get
            Set(value As String)
               _summary = value
            End Set
         End Property

         Private _targetId As Common.Generic.Nullable(Of String)
         Public Property TargetId As String
            Get
               Return If(_targetId.HasValue, _targetId.Value, _owner.TargetId.Value)
            End Get
            Set(value As String)
               _targetId = value
            End Set
         End Property

         Private _versionSeriesCheckedOutId As Common.Generic.Nullable(Of String)
         Public Property VersionSeriesCheckedOutId As String
            Get
               Return If(_versionSeriesCheckedOutId.HasValue, _versionSeriesCheckedOutId.Value, _owner.VersionSeriesCheckedOutId.Value)
            End Get
            Set(value As String)
               _versionSeriesCheckedOutId = value
            End Set
         End Property

         Private _versionSeriesId As Common.Generic.Nullable(Of String)
         Public Property VersionSeriesId As String
            Get
               Return If(_versionSeriesId.HasValue, _versionSeriesId.Value, _owner.VersionSeriesId.Value)
            End Get
            Set(value As String)
               _versionSeriesId = value
            End Set
         End Property

         ''' <summary>
         ''' Converts a date to a DateTimeOffset respecting the MinValue-restriction of DateTimeOffset
         ''' (lower and upper bounds given in utc; avoid ArgumentOutOfRangeException)
         ''' </summary>
         ''' <param name="value"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Private Shared Function ToDateTimeOffset(value As Date) As DateTimeOffset
            Try
               If value.Kind = DateTimeKind.Utc Then
                  Return value
               ElseIf value <= DateTimeOffset.MinValue.Date Then
                  Return DateTimeOffset.MinValue
               ElseIf value >= DateTimeOffset.MaxValue.Date Then
                  Return DateTimeOffset.MaxValue
               Else
                  Return value
               End If
            Catch ex As Exception
               Return If(value < Date.Now, DateTimeOffset.MinValue, DateTimeOffset.MaxValue)
            End Try
         End Function

      End Class
#End Region

#Region "IServiceModelObject"
      Public ReadOnly Property [Object] As cmisObjectType Implements Contracts.IServiceModelObject.Object
         Get
            Return Me
         End Get
      End Property

      Public ReadOnly Property PathSegment As String Implements Contracts.IServiceModelObject.PathSegment
         Get
            Return Nothing
         End Get
      End Property

      Public ReadOnly Property RelativePathSegment As String Implements Contracts.IServiceModelObject.RelativePathSegment
         Get
            Return Nothing
         End Get
      End Property

      Private ReadOnly _serviceModel As ServiceModelExtension
      Public ReadOnly Property ServiceModel As ServiceModelExtension Implements Contracts.IServiceModelObject.ServiceModel
         Get
            Return _serviceModel
         End Get
      End Property
#End Region

      ''' <summary>
      ''' Extension when cmisObjectType is used in BulkUpdateProperties call
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property BulkUpdateProperties As BulkUpdatePropertiesExtension
         Get
            Static retVal As New BulkUpdatePropertiesExtension(Me)
            Return retVal
         End Get
      End Property

   End Class
End Namespace