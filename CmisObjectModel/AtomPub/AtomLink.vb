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
Imports sss = System.ServiceModel.Syndication
Imports sx = System.Xml
Imports sxl = System.Xml.Linq
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.AtomPub
   Public Class AtomLink
      Inherits sss.SyndicationLink

#Region "Constants"
      Private Const csId As String = "id"
      Private Const csRenditionKind As String = "renditionKind"
#End Region

#Region "Constructors"
      Public Sub New()
         MyBase.New()

         'define prefixes for used namespaces
         For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
            Me.AttributeExtensions.Add(de.Key, de.Value)
         Next
      End Sub

      Public Sub New(uri As Uri)
         MyBase.New(uri)

         'define prefixes for used namespaces
         For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
            Me.AttributeExtensions.Add(de.Key, de.Value)
         Next
      End Sub

      Public Sub New(uri As Uri, relationshipType As String, mediaType As String)
         Me.New(uri)

         Me.RelationshipType = relationshipType
         Me.MediaType = mediaType
      End Sub

      Public Sub New(uri As Uri, relationshipType As String, mediaType As String,
                     id As String, renditionKind As String)
         Me.New(uri, relationshipType, mediaType)

         Me.Id = id
         Me.RenditionKind = renditionKind
      End Sub
#End Region

#Region "AtomPub-extensions"
      Private _id As sx.XmlQualifiedName
      ''' <summary>
      ''' AtomPub extension cmisra:Id
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property Id As String
         Get
            Return If(_id Is Nothing, Nothing, Me.AttributeExtensions(_id))
         End Get
         Private Set(value As String)
            If _id IsNot Nothing Then Me.AttributeExtensions.Remove(_id)
            If value Is Nothing Then
               _id = Nothing
            Else
               _id = New sx.XmlQualifiedName(csId, Constants.Namespaces.cmisra)
               Me.AttributeExtensions.Add(_id, value)
            End If
         End Set
      End Property 'Id

      Private _renditionKind As sx.XmlQualifiedName
      ''' <summary>
      ''' AtomPub extension cmisra:RenditionKind
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property RenditionKind As String
         Get
            Return If(_renditionKind Is Nothing, Nothing, AttributeExtensions(_renditionKind))
         End Get
         Set(value As String)
            If _renditionKind IsNot Nothing Then Me.AttributeExtensions.Remove(_renditionKind)
            If value Is Nothing Then
               _renditionKind = Nothing
            Else
               _renditionKind = New sx.XmlQualifiedName(csRenditionKind, Constants.Namespaces.cmisra)
               Me.AttributeExtensions.Add(_renditionKind, value)
            End If
         End Set
      End Property 'RenditionKind
#End Region

   End Class
End Namespace