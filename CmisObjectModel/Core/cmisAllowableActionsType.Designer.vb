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
Imports CmisObjectModel.Common
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisAllowableActionsType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   <System.CodeDom.Compiler.GeneratedCode("CmisXsdConverter", "1.0.0.0")>
   Partial Public Class cmisAllowableActionsType
      Inherits Serialization.XmlSerializable

      Public Sub New()
      End Sub
      ''' <summary>
      ''' this constructor is only used if derived classes from this class needs an InitClass()-call
      ''' </summary>
      ''' <param name="initClassSupported"></param>
      ''' <remarks></remarks>
      Protected Sub New(initClassSupported As Boolean?)
         MyBase.New(initClassSupported)
      End Sub

#Region "IXmlSerializable"
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisAllowableActionsType, String)) From {
         } '_setter

      ''' <summary>
      ''' Deserialization of all properties stored in attributes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
         'at least one property is serialized in an attribute-value
         If _setter.Count > 0 Then
            For attributeIndex As Integer = 0 To reader.AttributeCount - 1
               reader.MoveToAttribute(attributeIndex)
               'attribute name
               Dim key As String = reader.Name.ToLowerInvariant
               If _setter.ContainsKey(key) Then _setter(key).Invoke(Me, reader.GetAttribute(attributeIndex))
            Next
         End If
      End Sub

      ''' <summary>
      ''' Deserialization of all properties stored in subnodes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadXmlCore(reader As sx.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _canDeleteObject = Read(reader, attributeOverrides, "canDeleteObject", Constants.Namespaces.cmis, _canDeleteObject)
         _canUpdateProperties = Read(reader, attributeOverrides, "canUpdateProperties", Constants.Namespaces.cmis, _canUpdateProperties)
         _canGetFolderTree = Read(reader, attributeOverrides, "canGetFolderTree", Constants.Namespaces.cmis, _canGetFolderTree)
         _canGetProperties = Read(reader, attributeOverrides, "canGetProperties", Constants.Namespaces.cmis, _canGetProperties)
         _canGetObjectRelationships = Read(reader, attributeOverrides, "canGetObjectRelationships", Constants.Namespaces.cmis, _canGetObjectRelationships)
         _canGetObjectParents = Read(reader, attributeOverrides, "canGetObjectParents", Constants.Namespaces.cmis, _canGetObjectParents)
         _canGetFolderParent = Read(reader, attributeOverrides, "canGetFolderParent", Constants.Namespaces.cmis, _canGetFolderParent)
         _canGetDescendants = Read(reader, attributeOverrides, "canGetDescendants", Constants.Namespaces.cmis, _canGetDescendants)
         _canMoveObject = Read(reader, attributeOverrides, "canMoveObject", Constants.Namespaces.cmis, _canMoveObject)
         _canDeleteContentStream = Read(reader, attributeOverrides, "canDeleteContentStream", Constants.Namespaces.cmis, _canDeleteContentStream)
         _canCheckOut = Read(reader, attributeOverrides, "canCheckOut", Constants.Namespaces.cmis, _canCheckOut)
         _canCancelCheckOut = Read(reader, attributeOverrides, "canCancelCheckOut", Constants.Namespaces.cmis, _canCancelCheckOut)
         _canCheckIn = Read(reader, attributeOverrides, "canCheckIn", Constants.Namespaces.cmis, _canCheckIn)
         _canSetContentStream = Read(reader, attributeOverrides, "canSetContentStream", Constants.Namespaces.cmis, _canSetContentStream)
         _canGetAllVersions = Read(reader, attributeOverrides, "canGetAllVersions", Constants.Namespaces.cmis, _canGetAllVersions)
         _canAddObjectToFolder = Read(reader, attributeOverrides, "canAddObjectToFolder", Constants.Namespaces.cmis, _canAddObjectToFolder)
         _canRemoveObjectFromFolder = Read(reader, attributeOverrides, "canRemoveObjectFromFolder", Constants.Namespaces.cmis, _canRemoveObjectFromFolder)
         _canGetContentStream = Read(reader, attributeOverrides, "canGetContentStream", Constants.Namespaces.cmis, _canGetContentStream)
         _canApplyPolicy = Read(reader, attributeOverrides, "canApplyPolicy", Constants.Namespaces.cmis, _canApplyPolicy)
         _canGetAppliedPolicies = Read(reader, attributeOverrides, "canGetAppliedPolicies", Constants.Namespaces.cmis, _canGetAppliedPolicies)
         _canRemovePolicy = Read(reader, attributeOverrides, "canRemovePolicy", Constants.Namespaces.cmis, _canRemovePolicy)
         _canGetChildren = Read(reader, attributeOverrides, "canGetChildren", Constants.Namespaces.cmis, _canGetChildren)
         _canCreateDocument = Read(reader, attributeOverrides, "canCreateDocument", Constants.Namespaces.cmis, _canCreateDocument)
         _canCreateFolder = Read(reader, attributeOverrides, "canCreateFolder", Constants.Namespaces.cmis, _canCreateFolder)
         _canCreateRelationship = Read(reader, attributeOverrides, "canCreateRelationship", Constants.Namespaces.cmis, _canCreateRelationship)
         _canCreateItem = Read(reader, attributeOverrides, "canCreateItem", Constants.Namespaces.cmis, _canCreateItem)
         _canDeleteTree = Read(reader, attributeOverrides, "canDeleteTree", Constants.Namespaces.cmis, _canDeleteTree)
         _canGetRenditions = Read(reader, attributeOverrides, "canGetRenditions", Constants.Namespaces.cmis, _canGetRenditions)
         _canGetACL = Read(reader, attributeOverrides, "canGetACL", Constants.Namespaces.cmis, _canGetACL)
         _canApplyACL = Read(reader, attributeOverrides, "canApplyACL", Constants.Namespaces.cmis, _canApplyACL)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If _canDeleteObject.HasValue Then WriteElement(writer, attributeOverrides, "canDeleteObject", Constants.Namespaces.cmis, Convert(_canDeleteObject))
         If _canUpdateProperties.HasValue Then WriteElement(writer, attributeOverrides, "canUpdateProperties", Constants.Namespaces.cmis, Convert(_canUpdateProperties))
         If _canGetFolderTree.HasValue Then WriteElement(writer, attributeOverrides, "canGetFolderTree", Constants.Namespaces.cmis, Convert(_canGetFolderTree))
         If _canGetProperties.HasValue Then WriteElement(writer, attributeOverrides, "canGetProperties", Constants.Namespaces.cmis, Convert(_canGetProperties))
         If _canGetObjectRelationships.HasValue Then WriteElement(writer, attributeOverrides, "canGetObjectRelationships", Constants.Namespaces.cmis, Convert(_canGetObjectRelationships))
         If _canGetObjectParents.HasValue Then WriteElement(writer, attributeOverrides, "canGetObjectParents", Constants.Namespaces.cmis, Convert(_canGetObjectParents))
         If _canGetFolderParent.HasValue Then WriteElement(writer, attributeOverrides, "canGetFolderParent", Constants.Namespaces.cmis, Convert(_canGetFolderParent))
         If _canGetDescendants.HasValue Then WriteElement(writer, attributeOverrides, "canGetDescendants", Constants.Namespaces.cmis, Convert(_canGetDescendants))
         If _canMoveObject.HasValue Then WriteElement(writer, attributeOverrides, "canMoveObject", Constants.Namespaces.cmis, Convert(_canMoveObject))
         If _canDeleteContentStream.HasValue Then WriteElement(writer, attributeOverrides, "canDeleteContentStream", Constants.Namespaces.cmis, Convert(_canDeleteContentStream))
         If _canCheckOut.HasValue Then WriteElement(writer, attributeOverrides, "canCheckOut", Constants.Namespaces.cmis, Convert(_canCheckOut))
         If _canCancelCheckOut.HasValue Then WriteElement(writer, attributeOverrides, "canCancelCheckOut", Constants.Namespaces.cmis, Convert(_canCancelCheckOut))
         If _canCheckIn.HasValue Then WriteElement(writer, attributeOverrides, "canCheckIn", Constants.Namespaces.cmis, Convert(_canCheckIn))
         If _canSetContentStream.HasValue Then WriteElement(writer, attributeOverrides, "canSetContentStream", Constants.Namespaces.cmis, Convert(_canSetContentStream))
         If _canGetAllVersions.HasValue Then WriteElement(writer, attributeOverrides, "canGetAllVersions", Constants.Namespaces.cmis, Convert(_canGetAllVersions))
         If _canAddObjectToFolder.HasValue Then WriteElement(writer, attributeOverrides, "canAddObjectToFolder", Constants.Namespaces.cmis, Convert(_canAddObjectToFolder))
         If _canRemoveObjectFromFolder.HasValue Then WriteElement(writer, attributeOverrides, "canRemoveObjectFromFolder", Constants.Namespaces.cmis, Convert(_canRemoveObjectFromFolder))
         If _canGetContentStream.HasValue Then WriteElement(writer, attributeOverrides, "canGetContentStream", Constants.Namespaces.cmis, Convert(_canGetContentStream))
         If _canApplyPolicy.HasValue Then WriteElement(writer, attributeOverrides, "canApplyPolicy", Constants.Namespaces.cmis, Convert(_canApplyPolicy))
         If _canGetAppliedPolicies.HasValue Then WriteElement(writer, attributeOverrides, "canGetAppliedPolicies", Constants.Namespaces.cmis, Convert(_canGetAppliedPolicies))
         If _canRemovePolicy.HasValue Then WriteElement(writer, attributeOverrides, "canRemovePolicy", Constants.Namespaces.cmis, Convert(_canRemovePolicy))
         If _canGetChildren.HasValue Then WriteElement(writer, attributeOverrides, "canGetChildren", Constants.Namespaces.cmis, Convert(_canGetChildren))
         If _canCreateDocument.HasValue Then WriteElement(writer, attributeOverrides, "canCreateDocument", Constants.Namespaces.cmis, Convert(_canCreateDocument))
         If _canCreateFolder.HasValue Then WriteElement(writer, attributeOverrides, "canCreateFolder", Constants.Namespaces.cmis, Convert(_canCreateFolder))
         If _canCreateRelationship.HasValue Then WriteElement(writer, attributeOverrides, "canCreateRelationship", Constants.Namespaces.cmis, Convert(_canCreateRelationship))
         If _canCreateItem.HasValue Then WriteElement(writer, attributeOverrides, "canCreateItem", Constants.Namespaces.cmis, Convert(_canCreateItem))
         If _canDeleteTree.HasValue Then WriteElement(writer, attributeOverrides, "canDeleteTree", Constants.Namespaces.cmis, Convert(_canDeleteTree))
         If _canGetRenditions.HasValue Then WriteElement(writer, attributeOverrides, "canGetRenditions", Constants.Namespaces.cmis, Convert(_canGetRenditions))
         If _canGetACL.HasValue Then WriteElement(writer, attributeOverrides, "canGetACL", Constants.Namespaces.cmis, Convert(_canGetACL))
         If _canApplyACL.HasValue Then WriteElement(writer, attributeOverrides, "canApplyACL", Constants.Namespaces.cmis, Convert(_canApplyACL))
      End Sub
#End Region

      Protected _canAddObjectToFolder As Boolean?
      Public Overridable Property CanAddObjectToFolder As Boolean?
         Get
            Return _canAddObjectToFolder
         End Get
         Set(value As Boolean?)
            If Not _canAddObjectToFolder.Equals(value) Then
               Dim oldValue As Boolean? = _canAddObjectToFolder
               _canAddObjectToFolder = value
               OnPropertyChanged("CanAddObjectToFolder", value, oldValue)
            End If
         End Set
      End Property 'CanAddObjectToFolder

      Protected _canApplyACL As Boolean?
      Public Overridable Property CanApplyACL As Boolean?
         Get
            Return _canApplyACL
         End Get
         Set(value As Boolean?)
            If Not _canApplyACL.Equals(value) Then
               Dim oldValue As Boolean? = _canApplyACL
               _canApplyACL = value
               OnPropertyChanged("CanApplyACL", value, oldValue)
            End If
         End Set
      End Property 'CanApplyACL

      Protected _canApplyPolicy As Boolean?
      Public Overridable Property CanApplyPolicy As Boolean?
         Get
            Return _canApplyPolicy
         End Get
         Set(value As Boolean?)
            If Not _canApplyPolicy.Equals(value) Then
               Dim oldValue As Boolean? = _canApplyPolicy
               _canApplyPolicy = value
               OnPropertyChanged("CanApplyPolicy", value, oldValue)
            End If
         End Set
      End Property 'CanApplyPolicy

      Protected _canCancelCheckOut As Boolean?
      Public Overridable Property CanCancelCheckOut As Boolean?
         Get
            Return _canCancelCheckOut
         End Get
         Set(value As Boolean?)
            If Not _canCancelCheckOut.Equals(value) Then
               Dim oldValue As Boolean? = _canCancelCheckOut
               _canCancelCheckOut = value
               OnPropertyChanged("CanCancelCheckOut", value, oldValue)
            End If
         End Set
      End Property 'CanCancelCheckOut

      Protected _canCheckIn As Boolean?
      Public Overridable Property CanCheckIn As Boolean?
         Get
            Return _canCheckIn
         End Get
         Set(value As Boolean?)
            If Not _canCheckIn.Equals(value) Then
               Dim oldValue As Boolean? = _canCheckIn
               _canCheckIn = value
               OnPropertyChanged("CanCheckIn", value, oldValue)
            End If
         End Set
      End Property 'CanCheckIn

      Protected _canCheckOut As Boolean?
      Public Overridable Property CanCheckOut As Boolean?
         Get
            Return _canCheckOut
         End Get
         Set(value As Boolean?)
            If Not _canCheckOut.Equals(value) Then
               Dim oldValue As Boolean? = _canCheckOut
               _canCheckOut = value
               OnPropertyChanged("CanCheckOut", value, oldValue)
            End If
         End Set
      End Property 'CanCheckOut

      Protected _canCreateDocument As Boolean?
      Public Overridable Property CanCreateDocument As Boolean?
         Get
            Return _canCreateDocument
         End Get
         Set(value As Boolean?)
            If Not _canCreateDocument.Equals(value) Then
               Dim oldValue As Boolean? = _canCreateDocument
               _canCreateDocument = value
               OnPropertyChanged("CanCreateDocument", value, oldValue)
            End If
         End Set
      End Property 'CanCreateDocument

      Protected _canCreateFolder As Boolean?
      Public Overridable Property CanCreateFolder As Boolean?
         Get
            Return _canCreateFolder
         End Get
         Set(value As Boolean?)
            If Not _canCreateFolder.Equals(value) Then
               Dim oldValue As Boolean? = _canCreateFolder
               _canCreateFolder = value
               OnPropertyChanged("CanCreateFolder", value, oldValue)
            End If
         End Set
      End Property 'CanCreateFolder

      Protected _canCreateItem As Boolean?
      Public Overridable Property CanCreateItem As Boolean?
         Get
            Return _canCreateItem
         End Get
         Set(value As Boolean?)
            If Not _canCreateItem.Equals(value) Then
               Dim oldValue As Boolean? = _canCreateItem
               _canCreateItem = value
               OnPropertyChanged("CanCreateItem", value, oldValue)
            End If
         End Set
      End Property 'CanCreateItem

      Protected _canCreateRelationship As Boolean?
      Public Overridable Property CanCreateRelationship As Boolean?
         Get
            Return _canCreateRelationship
         End Get
         Set(value As Boolean?)
            If Not _canCreateRelationship.Equals(value) Then
               Dim oldValue As Boolean? = _canCreateRelationship
               _canCreateRelationship = value
               OnPropertyChanged("CanCreateRelationship", value, oldValue)
            End If
         End Set
      End Property 'CanCreateRelationship

      Protected _canDeleteContentStream As Boolean?
      Public Overridable Property CanDeleteContentStream As Boolean?
         Get
            Return _canDeleteContentStream
         End Get
         Set(value As Boolean?)
            If Not _canDeleteContentStream.Equals(value) Then
               Dim oldValue As Boolean? = _canDeleteContentStream
               _canDeleteContentStream = value
               OnPropertyChanged("CanDeleteContentStream", value, oldValue)
            End If
         End Set
      End Property 'CanDeleteContentStream

      Protected _canDeleteObject As Boolean?
      Public Overridable Property CanDeleteObject As Boolean?
         Get
            Return _canDeleteObject
         End Get
         Set(value As Boolean?)
            If Not _canDeleteObject.Equals(value) Then
               Dim oldValue As Boolean? = _canDeleteObject
               _canDeleteObject = value
               OnPropertyChanged("CanDeleteObject", value, oldValue)
            End If
         End Set
      End Property 'CanDeleteObject

      Protected _canDeleteTree As Boolean?
      Public Overridable Property CanDeleteTree As Boolean?
         Get
            Return _canDeleteTree
         End Get
         Set(value As Boolean?)
            If Not _canDeleteTree.Equals(value) Then
               Dim oldValue As Boolean? = _canDeleteTree
               _canDeleteTree = value
               OnPropertyChanged("CanDeleteTree", value, oldValue)
            End If
         End Set
      End Property 'CanDeleteTree

      Protected _canGetACL As Boolean?
      Public Overridable Property CanGetACL As Boolean?
         Get
            Return _canGetACL
         End Get
         Set(value As Boolean?)
            If Not _canGetACL.Equals(value) Then
               Dim oldValue As Boolean? = _canGetACL
               _canGetACL = value
               OnPropertyChanged("CanGetACL", value, oldValue)
            End If
         End Set
      End Property 'CanGetACL

      Protected _canGetAllVersions As Boolean?
      Public Overridable Property CanGetAllVersions As Boolean?
         Get
            Return _canGetAllVersions
         End Get
         Set(value As Boolean?)
            If Not _canGetAllVersions.Equals(value) Then
               Dim oldValue As Boolean? = _canGetAllVersions
               _canGetAllVersions = value
               OnPropertyChanged("CanGetAllVersions", value, oldValue)
            End If
         End Set
      End Property 'CanGetAllVersions

      Protected _canGetAppliedPolicies As Boolean?
      Public Overridable Property CanGetAppliedPolicies As Boolean?
         Get
            Return _canGetAppliedPolicies
         End Get
         Set(value As Boolean?)
            If Not _canGetAppliedPolicies.Equals(value) Then
               Dim oldValue As Boolean? = _canGetAppliedPolicies
               _canGetAppliedPolicies = value
               OnPropertyChanged("CanGetAppliedPolicies", value, oldValue)
            End If
         End Set
      End Property 'CanGetAppliedPolicies

      Protected _canGetChildren As Boolean?
      Public Overridable Property CanGetChildren As Boolean?
         Get
            Return _canGetChildren
         End Get
         Set(value As Boolean?)
            If Not _canGetChildren.Equals(value) Then
               Dim oldValue As Boolean? = _canGetChildren
               _canGetChildren = value
               OnPropertyChanged("CanGetChildren", value, oldValue)
            End If
         End Set
      End Property 'CanGetChildren

      Protected _canGetContentStream As Boolean?
      Public Overridable Property CanGetContentStream As Boolean?
         Get
            Return _canGetContentStream
         End Get
         Set(value As Boolean?)
            If Not _canGetContentStream.Equals(value) Then
               Dim oldValue As Boolean? = _canGetContentStream
               _canGetContentStream = value
               OnPropertyChanged("CanGetContentStream", value, oldValue)
            End If
         End Set
      End Property 'CanGetContentStream

      Protected _canGetDescendants As Boolean?
      Public Overridable Property CanGetDescendants As Boolean?
         Get
            Return _canGetDescendants
         End Get
         Set(value As Boolean?)
            If Not _canGetDescendants.Equals(value) Then
               Dim oldValue As Boolean? = _canGetDescendants
               _canGetDescendants = value
               OnPropertyChanged("CanGetDescendants", value, oldValue)
            End If
         End Set
      End Property 'CanGetDescendants

      Protected _canGetFolderParent As Boolean?
      Public Overridable Property CanGetFolderParent As Boolean?
         Get
            Return _canGetFolderParent
         End Get
         Set(value As Boolean?)
            If Not _canGetFolderParent.Equals(value) Then
               Dim oldValue As Boolean? = _canGetFolderParent
               _canGetFolderParent = value
               OnPropertyChanged("CanGetFolderParent", value, oldValue)
            End If
         End Set
      End Property 'CanGetFolderParent

      Protected _canGetFolderTree As Boolean?
      Public Overridable Property CanGetFolderTree As Boolean?
         Get
            Return _canGetFolderTree
         End Get
         Set(value As Boolean?)
            If Not _canGetFolderTree.Equals(value) Then
               Dim oldValue As Boolean? = _canGetFolderTree
               _canGetFolderTree = value
               OnPropertyChanged("CanGetFolderTree", value, oldValue)
            End If
         End Set
      End Property 'CanGetFolderTree

      Protected _canGetObjectParents As Boolean?
      Public Overridable Property CanGetObjectParents As Boolean?
         Get
            Return _canGetObjectParents
         End Get
         Set(value As Boolean?)
            If Not _canGetObjectParents.Equals(value) Then
               Dim oldValue As Boolean? = _canGetObjectParents
               _canGetObjectParents = value
               OnPropertyChanged("CanGetObjectParents", value, oldValue)
            End If
         End Set
      End Property 'CanGetObjectParents

      Protected _canGetObjectRelationships As Boolean?
      Public Overridable Property CanGetObjectRelationships As Boolean?
         Get
            Return _canGetObjectRelationships
         End Get
         Set(value As Boolean?)
            If Not _canGetObjectRelationships.Equals(value) Then
               Dim oldValue As Boolean? = _canGetObjectRelationships
               _canGetObjectRelationships = value
               OnPropertyChanged("CanGetObjectRelationships", value, oldValue)
            End If
         End Set
      End Property 'CanGetObjectRelationships

      Protected _canGetProperties As Boolean?
      Public Overridable Property CanGetProperties As Boolean?
         Get
            Return _canGetProperties
         End Get
         Set(value As Boolean?)
            If Not _canGetProperties.Equals(value) Then
               Dim oldValue As Boolean? = _canGetProperties
               _canGetProperties = value
               OnPropertyChanged("CanGetProperties", value, oldValue)
            End If
         End Set
      End Property 'CanGetProperties

      Protected _canGetRenditions As Boolean?
      Public Overridable Property CanGetRenditions As Boolean?
         Get
            Return _canGetRenditions
         End Get
         Set(value As Boolean?)
            If Not _canGetRenditions.Equals(value) Then
               Dim oldValue As Boolean? = _canGetRenditions
               _canGetRenditions = value
               OnPropertyChanged("CanGetRenditions", value, oldValue)
            End If
         End Set
      End Property 'CanGetRenditions

      Protected _canMoveObject As Boolean?
      Public Overridable Property CanMoveObject As Boolean?
         Get
            Return _canMoveObject
         End Get
         Set(value As Boolean?)
            If Not _canMoveObject.Equals(value) Then
               Dim oldValue As Boolean? = _canMoveObject
               _canMoveObject = value
               OnPropertyChanged("CanMoveObject", value, oldValue)
            End If
         End Set
      End Property 'CanMoveObject

      Protected _canRemoveObjectFromFolder As Boolean?
      Public Overridable Property CanRemoveObjectFromFolder As Boolean?
         Get
            Return _canRemoveObjectFromFolder
         End Get
         Set(value As Boolean?)
            If Not _canRemoveObjectFromFolder.Equals(value) Then
               Dim oldValue As Boolean? = _canRemoveObjectFromFolder
               _canRemoveObjectFromFolder = value
               OnPropertyChanged("CanRemoveObjectFromFolder", value, oldValue)
            End If
         End Set
      End Property 'CanRemoveObjectFromFolder

      Protected _canRemovePolicy As Boolean?
      Public Overridable Property CanRemovePolicy As Boolean?
         Get
            Return _canRemovePolicy
         End Get
         Set(value As Boolean?)
            If Not _canRemovePolicy.Equals(value) Then
               Dim oldValue As Boolean? = _canRemovePolicy
               _canRemovePolicy = value
               OnPropertyChanged("CanRemovePolicy", value, oldValue)
            End If
         End Set
      End Property 'CanRemovePolicy

      Protected _canSetContentStream As Boolean?
      Public Overridable Property CanSetContentStream As Boolean?
         Get
            Return _canSetContentStream
         End Get
         Set(value As Boolean?)
            If Not _canSetContentStream.Equals(value) Then
               Dim oldValue As Boolean? = _canSetContentStream
               _canSetContentStream = value
               OnPropertyChanged("CanSetContentStream", value, oldValue)
            End If
         End Set
      End Property 'CanSetContentStream

      Protected _canUpdateProperties As Boolean?
      Public Overridable Property CanUpdateProperties As Boolean?
         Get
            Return _canUpdateProperties
         End Get
         Set(value As Boolean?)
            If Not _canUpdateProperties.Equals(value) Then
               Dim oldValue As Boolean? = _canUpdateProperties
               _canUpdateProperties = value
               OnPropertyChanged("CanUpdateProperties", value, oldValue)
            End If
         End Set
      End Property 'CanUpdateProperties

   End Class
End Namespace