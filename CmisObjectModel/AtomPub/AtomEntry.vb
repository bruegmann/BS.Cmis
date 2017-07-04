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
Imports sss = System.ServiceModel.Syndication
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.AtomPub
   ''' <summary>
   ''' Represents a cmisObject (for example: cmisdocument)
   ''' </summary>
   ''' <remarks></remarks>
   Public Class AtomEntry
      Inherits sss.SyndicationItem

#Region "Constants"
      Private Const csBulkUpdate As String = "bulkUpdate"
      Private Const csChildren As String = "children"
      Private Const csContent As String = "content"
      Private Const csObject As String = "object"
      Private Const csPathSegment As String = "pathSegment"
      Private Const csRelativePathSegment As String = "relativePathSegment"
      Private Const csType As String = "type"
#End Region

#Region "Constructors"
      Public Sub New()
         MyBase.New()

         'define prefixes for used namespaces
         For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
            Me.AttributeExtensions.Add(de.Key, de.Value)
         Next
      End Sub

      Friend Sub New(bulkUpdate As Core.cmisBulkUpdateType, Optional links As List(Of AtomLink) = Nothing)
         Me.New()

         Dim currentTime As DateTimeOffset = DateTimeOffset.UtcNow
         InitClass("urn:bulkupdate", Nothing, Nothing, currentTime, currentTime, links)
         Me.BulkUpdate = bulkUpdate
      End Sub

      Friend Sub New(objectId As String)
         Me.New(New Core.cmisObjectType() With {.ObjectId = objectId})
      End Sub

      Friend Sub New(cmisraObject As Core.cmisObjectType, Optional content As RestAtom.cmisContentType = Nothing)
         Me.New()

         Dim currentTime As DateTimeOffset = DateTimeOffset.UtcNow
         InitClass(cmisraObject, "", "", "", currentTime, currentTime, Nothing)

         If content IsNot Nothing Then Me.Content = content
      End Sub

      Friend Sub New(cmisraType As Core.Definitions.Types.cmisTypeDefinitionType)
         Me.New()

         Dim currentTime As DateTimeOffset = DateTimeOffset.UtcNow
         InitClass(cmisraType, "", "", "", currentTime, currentTime, Nothing)
      End Sub

      Public Sub New(cmisraType As Core.Definitions.Types.cmisTypeDefinitionType,
                     links As List(Of AtomLink), ParamArray authors As sss.SyndicationPerson())
         Me.New()

         Dim currentTime As DateTimeOffset = DateTimeOffset.UtcNow
         InitClass(cmisraType, "", "", "", currentTime, currentTime, links, authors)
      End Sub

      Public Sub New(cmisraType As Core.Definitions.Types.cmisTypeDefinitionType,
                     children As AtomFeed, links As List(Of AtomLink),
                     ParamArray authors As sss.SyndicationPerson())
         Me.New()

         Dim currentTime As DateTimeOffset = DateTimeOffset.UtcNow
         InitClass(cmisraType, "", "", "", currentTime, currentTime, links, authors)
         Me.Children = children
      End Sub

      Public Sub New(id As String, title As String, summary As String,
                     publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                     links As List(Of AtomLink),
                     ParamArray authors As sss.SyndicationPerson())
         Me.New()
         InitClass(id, title, summary, publishDate, lastUpdatedTime, links, authors)
      End Sub

      Protected Sub New(id As String, title As String, summary As String,
                        publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                        cmisraObject As Core.cmisObjectType, contentLink As AtomLink, content As RestAtom.cmisContentType,
                        children As AtomFeed, links As List(Of AtomLink),
                        relativePathSegment As String, pathSegment As String,
                        ParamArray authors As sss.SyndicationPerson())
         Me.New()
         InitClass(cmisraObject, id, title, summary, publishDate, lastUpdatedTime, links, authors)

         If contentLink Is Nothing Then
            Me.Content = content
         Else
            Me.ContentLink = contentLink
         End If
         Me.Children = children
         Me.PathSegment = pathSegment
         Me.RelativePathSegment = relativePathSegment
      End Sub
      Public Sub New(id As String, title As String, summary As String,
                     publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                     cmisraObject As Core.cmisObjectType, contentLink As AtomLink,
                     children As AtomFeed, links As List(Of AtomLink),
                     relativePathSegment As String, pathSegment As String,
                     ParamArray authors As sss.SyndicationPerson())
         Me.New(id, title, summary,
                publishDate, lastUpdatedTime,
                cmisraObject, contentLink, Nothing,
                children, links, relativePathSegment, pathSegment, authors)
      End Sub
      Public Sub New(id As String, title As String, summary As String,
                     publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                     cmisraObject As Core.cmisObjectType, content As RestAtom.cmisContentType,
                     children As AtomFeed, links As List(Of AtomLink),
                     relativePathSegment As String, pathSegment As String,
                     ParamArray authors As sss.SyndicationPerson())
         Me.New(id, title, summary,
                publishDate, lastUpdatedTime,
                cmisraObject, Nothing, content,
                children, links, relativePathSegment, pathSegment, authors)
      End Sub

      Public Sub New(id As String, title As String, summary As String,
                     publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                     cmisraType As Core.Definitions.Types.cmisTypeDefinitionType,
                     children As AtomFeed, links As List(Of AtomLink),
                     ParamArray authors As sss.SyndicationPerson())
         Me.New()
         InitClass(cmisraType, id, title, summary, publishDate, lastUpdatedTime, links, authors)

         Me.Children = children
      End Sub

      ''' <summary>
      ''' Creates a new instance (similar to ReadXml() in IXmlSerializable-classes)
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(reader As sx.XmlReader) As AtomEntry
         Dim authors As New List(Of sss.SyndicationPerson)
         Dim bulkUpdate As Core.cmisBulkUpdateType = Nothing
         Dim children As AtomFeed = Nothing
         Dim content As RestAtom.cmisContentType = Nothing
         Dim contentLink As AtomLink = Nothing
         Dim id As String = Nothing
         Dim lastUpdatedTime As DateTimeOffset
         Dim links As New List(Of AtomLink)
         Dim [object] As Core.cmisObjectType = Nothing
         Dim pathSegment As String = Nothing
         Dim publishDate As DateTimeOffset
         Dim relativePathSegment As String = Nothing
         Dim summary As String = Nothing
         Dim title As String = Nothing
         Dim type As Core.Definitions.Types.cmisTypeDefinitionType = Nothing
         Dim isEmptyElement As Boolean

         reader.MoveToContent()
         isEmptyElement = reader.IsEmptyElement
         reader.ReadStartElement()
         If Not isEmptyElement Then
            reader.MoveToContent()
            While reader.IsStartElement
               Select Case reader.NamespaceURI
                  Case Constants.Namespaces.atom
                     Select Case reader.LocalName
                        Case "author", "contributor"
                           authors.Add(Factory.CreateAuthor(reader))
                        Case csContent
                           contentLink = Factory.CreateLink(reader)
                        Case "id"
                           id = reader.ReadElementString()
                        Case "link"
                           links.Add(Factory.CreateLink(reader))
                        Case "published"
                           publishDate = Factory.CreateDateTimeOffset(reader)
                        Case "summary"
                           summary = reader.ReadElementString()
                        Case "title"
                           title = reader.ReadElementString()
                        Case "updated"
                           lastUpdatedTime = Factory.CreateDateTimeOffset(reader)
                        Case Else
                           'ignore node
                           reader.ReadOuterXml()
                     End Select
                  Case Constants.Namespaces.app
                     Select Case reader.LocalName
                        Case "edited"
                           'same as 'updated'
                           reader.ReadElementString()
                        Case Else
                           'ignore node
                           reader.ReadOuterXml()
                     End Select
                  Case Constants.Namespaces.cmisra
                     Select Case reader.LocalName
                        Case csBulkUpdate
                           bulkUpdate = New Core.cmisBulkUpdateType
                           bulkUpdate.ReadXml(reader)
                        Case csChildren
                           If reader.IsEmptyElement Then
                              'ignore node
                              reader.ReadOuterXml()
                           Else
                              reader.ReadStartElement()
                              reader.MoveToContent()
                              While reader.IsStartElement
                                 If reader.NamespaceURI = Constants.Namespaces.atom AndAlso reader.LocalName = "feed" Then
                                    children = AtomFeed.CreateInstance(reader)
                                 Else
                                    'ignore node
                                    reader.ReadOuterXml()
                                 End If
                                 reader.MoveToContent()
                              End While
                              reader.ReadEndElement()
                           End If
                        Case csContent
                           content = New RestAtom.cmisContentType
                           content.ReadXml(reader)
                        Case csObject
                           [object] = New Core.cmisObjectType
                           [object].ReadXml(reader)
                        Case csPathSegment
                           pathSegment = reader.ReadElementString()
                        Case csRelativePathSegment
                           relativePathSegment = reader.ReadElementString()
                        Case csType
                           type = Core.Definitions.Types.cmisTypeDefinitionType.CreateInstance(reader)
                        Case Else
                           'ignore node
                           reader.ReadOuterXml()
                     End Select
                  Case Else
                     'ignore node
                     reader.ReadOuterXml()
               End Select
               reader.MoveToContent()
            End While

            reader.ReadEndElement()
         End If

         If type Is Nothing Then
            Return New AtomEntry(id, title, summary,
                                 publishDate, lastUpdatedTime,
                                 [object], contentLink, content, children,
                                 If(links.Count > 0, links, Nothing),
                                 relativePathSegment, pathSegment,
                                 If(authors.Count > 0, authors.ToArray(), Nothing)) With {.BulkUpdate = bulkUpdate}
         Else
            Return New AtomEntry(id, title, summary,
                                 publishDate, lastUpdatedTime, type, children,
                                 If(links.Count > 0, links, Nothing),
                                 If(authors.Count > 0, authors.ToArray(), Nothing))
         End If
      End Function

      Private Sub InitClass(id As String, title As String, summary As String,
                            publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                            links As List(Of AtomLink),
                            ParamArray authors As sss.SyndicationPerson())
         Me.Id = id
         If Not String.IsNullOrEmpty(title) Then Me.Title = New sss.TextSyndicationContent(title)
         If Not String.IsNullOrEmpty(summary) Then Me.Summary = New sss.TextSyndicationContent(summary)
         Me.PublishDate = publishDate
         Me.LastUpdatedTime = lastUpdatedTime
         '3.5.2 Entries
         'app:edited MUST be lastModificationDate
         Me.ElementExtensions.Add("edited", Constants.Namespaces.app, sx.XmlConvert.ToString(lastUpdatedTime))
         If authors IsNot Nothing Then
            For Each author As sss.SyndicationPerson In authors
               Me.Authors.Add(author)
            Next
         End If
         If links IsNot Nothing Then
            For Each link As AtomLink In links
               Me.Links.Add(link)
            Next
         End If
      End Sub

      ''' <summary>
      ''' Initializes this instance and ensures, that the AtomEntry is conform to the guidelines
      ''' performed in 3.5.2 Entries
      ''' </summary>
      ''' <param name="cmisraObject"></param>
      ''' <param name="id"></param>
      ''' <param name="title"></param>
      ''' <param name="summary"></param>
      ''' <param name="publishDate"></param>
      ''' <param name="lastUpdatedTime"></param>
      ''' <param name="links"></param>
      ''' <param name="authors"></param>
      ''' <remarks></remarks>
      Private Sub InitClass(cmisraObject As Core.cmisObjectType,
                            id As String, title As String, summary As String,
                            publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                            links As List(Of AtomLink),
                            ParamArray authors As sss.SyndicationPerson())
         Me.Object = cmisraObject
         'guidelines
         If cmisraObject IsNot Nothing AndAlso cmisraObject.Properties IsNot Nothing Then
            Dim author As String
            Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) =
               cmisraObject.Properties.FindProperties(True,
                                                      CmisPredefinedPropertyNames.Name,
                                                      CmisPredefinedPropertyNames.LastModificationDate,
                                                      CmisPredefinedPropertyNames.CreationDate,
                                                      CmisPredefinedPropertyNames.CreatedBy,
                                                      CmisPredefinedPropertyNames.Description,
                                                      CmisPredefinedPropertyNames.ObjectId)
            'atom:title MUST be the cmis:name property
            title = ReadProperty(properties(CmisPredefinedPropertyNames.Name), title)
            'atom:updated MUST be cmis:lastModificationDate
            lastUpdatedTime = ReadProperty(properties(CmisPredefinedPropertyNames.LastModificationDate), lastUpdatedTime)
            'atom:published MUST be cmis:creationDate
            publishDate = ReadProperty(properties(CmisPredefinedPropertyNames.CreationDate), publishDate)
            'atom:author/atom:name MUST be cmis:createdBy
            author = ReadProperty(properties(CmisPredefinedPropertyNames.CreatedBy), "")
            If Not String.IsNullOrEmpty(author) Then
               If authors Is Nothing OrElse authors.Length = 0 OrElse String.Compare(authors(0).Name, author, True) <> 0 Then
                  authors = New sss.SyndicationPerson() {New sss.SyndicationPerson(Nothing, author, Nothing)}
               End If
            End If
            'atom:summary SHOULD be cmis:description
            If String.IsNullOrEmpty(summary) Then summary = ReadProperty(properties(CmisPredefinedPropertyNames.Description), "")
            'atom:id SHOULD be derived from cmis:objectId
            If String.IsNullOrEmpty(id) Then id = "urn:objects:" & ReadProperty(properties(CmisPredefinedPropertyNames.ObjectId), "id")
         End If

         InitClass(id, title, summary, publishDate, lastUpdatedTime, links, authors)
      End Sub

      ''' <summary>
      ''' Initializes this instance and ensures, that the AtomEntry is conform to the guidelines
      ''' performed in 3.5.2 Entries
      ''' </summary>
      ''' <param name="cmisraType"></param>
      ''' <param name="id"></param>
      ''' <param name="title"></param>
      ''' <param name="summary"></param>
      ''' <param name="publishDate"></param>
      ''' <param name="lastUpdatedTime"></param>
      ''' <param name="links"></param>
      ''' <param name="authors"></param>
      ''' <remarks></remarks>
      Private Sub InitClass(cmisraType As Core.Definitions.Types.cmisTypeDefinitionType,
                            id As String, title As String, summary As String,
                            publishDate As DateTimeOffset, lastUpdatedTime As DateTimeOffset,
                            links As List(Of AtomLink),
                            ParamArray authors As sss.SyndicationPerson())
         Me.Type = cmisraType
         If cmisraType IsNot Nothing Then
            'atom:title MUST be the cmis:displayName
            title = cmisraType.DisplayName
            'The repository SHOULD populate the atom:summary tag with text that best represents a summary of the object. For example, the type description if available
            If String.IsNullOrEmpty(summary) Then summary = cmisraType.Description.NVL(title, cmisraType.Id)
            If String.IsNullOrEmpty(id) Then id = "urn:types:" & cmisraType.Id
         End If

         InitClass(id, title, summary, publishDate, lastUpdatedTime, links, authors)
      End Sub

      ''' <summary>
      ''' Returns [property].Value or defaultValue if [property].Value equals to null
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="property"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Function ReadProperty(Of TResult)([property] As Core.Properties.cmisProperty,
                                                defaultValue As TResult) As TResult
         Dim value As TResult = If(TypeOf [property] Is Core.Properties.Generic.cmisProperty(Of TResult),
                                   CType([property], Core.Properties.Generic.cmisProperty(Of TResult)).Value, Nothing)
         Return If(value Is Nothing OrElse value.Equals(Nothing), defaultValue, value)
      End Function
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Implements the children - AtomPub extension defined in
      ''' http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-RestAtom.xsd
      ''' </summary>
      ''' <remarks></remarks>
      Public Class CmisChildrenType
         Inherits Serialization.XmlSerializable

         Public Sub New()
         End Sub
         Public Sub New(children As AtomFeed)
            feedFormatter = New sss.Atom10FeedFormatter(children)
         End Sub

         Public Property feedFormatter As sss.Atom10FeedFormatter

         Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
            'nicht benötigt
         End Sub

         Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
            'nicht benötigt (siehe Methode AtomEntry.CreateInstance(), in der AtomFeed.CreateInstance() aufgerufen wird)
         End Sub

         Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
            If feedFormatter IsNot Nothing Then
               Dim xmlDoc As sx.XmlDocument = Serialization.SerializationHelper.ToXmlDocument(feedFormatter, attributeOverrides)
               writer.WriteRaw(xmlDoc.DocumentElement.OuterXml)
            End If
         End Sub
      End Class
#End Region

#Region "AtomPub-extensions"
      Private _bulkUpdate As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension BulkUpdate
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property BulkUpdate As Core.cmisBulkUpdateType
         Get
            Return If(_bulkUpdate Is Nothing, Nothing, _bulkUpdate.GetObject(Of Core.cmisBulkUpdateType))
         End Get
         Private Set(value As Core.cmisBulkUpdateType)
            If _bulkUpdate IsNot Nothing Then Me.ElementExtensions.Remove(_bulkUpdate)
            If value Is Nothing Then
               _bulkUpdate = Nothing
            Else
               _bulkUpdate = New sss.SyndicationElementExtension(csBulkUpdate, Constants.Namespaces.cmisra, value)
               Me.ElementExtensions.Add(_bulkUpdate)
            End If
         End Set
      End Property 'BulkUpdate

      Private _children As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension Children
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property Children As AtomFeed
         Get
            Return If(_children Is Nothing, Nothing, CType(_children.GetObject(Of CmisChildrenType).feedFormatter.Feed, AtomFeed))
         End Get
         Private Set(value As AtomFeed)
            If _children IsNot Nothing Then Me.ElementExtensions.Remove(_children)
            If value Is Nothing Then
               _children = Nothing
            Else
               'omit duplicate namespace definitions
               For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In Common.CmisNamespaces
                  value.AttributeExtensions.Remove(de.Key)
               Next
               _children = New sss.SyndicationElementExtension(csChildren, Constants.Namespaces.cmisra, New CmisChildrenType(value))
               Me.ElementExtensions.Add(_children)
            End If
         End Set
      End Property 'Children

      Private _content As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension Content
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Shadows Property Content As RestAtom.cmisContentType
         Get
            Return If(_content Is Nothing, Nothing, _content.GetObject(Of RestAtom.cmisContentType))
         End Get
         Private Set(value As RestAtom.cmisContentType)
            If _content IsNot Nothing Then Me.ElementExtensions.Remove(_content)
            If value Is Nothing Then
               _content = Nothing
            Else
               _content = New sss.SyndicationElementExtension(value)
               Me.ElementExtensions.Add(_content)
            End If
         End Set
      End Property 'Content

      Private _object As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension Object
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property [Object] As Core.cmisObjectType
         Get
            Return If(_object Is Nothing, Nothing, _object.GetObject(Of Core.cmisObjectType))
         End Get
         Private Set(value As Core.cmisObjectType)
            If _object IsNot Nothing Then Me.ElementExtensions.Remove(_object)
            If value Is Nothing Then
               _object = Nothing
            Else
               _object = New sss.SyndicationElementExtension(csObject, Constants.Namespaces.cmisra, value)
               Me.ElementExtensions.Add(_object)
            End If
         End Set
      End Property '[Object]

      Private _pathSegment As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension PathSegment
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property PathSegment As String
         Get
            Return If(_pathSegment Is Nothing, Nothing, _pathSegment.GetObject(Of String))
         End Get
         Private Set(value As String)
            If _pathSegment IsNot Nothing Then Me.ElementExtensions.Remove(_pathSegment)
            If String.IsNullOrEmpty(value) Then
               _pathSegment = Nothing
            Else
               _pathSegment = New sss.SyndicationElementExtension(csPathSegment, Constants.Namespaces.cmisra, value)
               Me.ElementExtensions.Add(_pathSegment)
            End If
         End Set
      End Property 'PathSegment

      Private _relativePathSegment As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub - extension RelativePathSegment
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property RelativePathSegment As String
         Get
            Return If(_relativePathSegment Is Nothing, Nothing, _relativePathSegment.GetObject(Of String))
         End Get
         Private Set(value As String)
            If _relativePathSegment IsNot Nothing Then Me.ElementExtensions.Remove(_relativePathSegment)
            If String.IsNullOrEmpty(value) Then
               _relativePathSegment = Nothing
            Else
               _relativePathSegment = New sss.SyndicationElementExtension(csRelativePathSegment, Constants.Namespaces.cmisra, value)
               Me.ElementExtensions.Add(_relativePathSegment)
            End If
         End Set
      End Property 'RelativePathSegment

      Private _type As sss.SyndicationElementExtension
      ''' <summary>
      ''' AtomPub extension Type
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public Property Type As Core.Definitions.Types.cmisTypeDefinitionType
         Get
            Return If(_type Is Nothing, Nothing, _type.GetObject(Of Core.Definitions.Types.cmisTypeDefinitionType))
         End Get
         Private Set(value As Core.Definitions.Types.cmisTypeDefinitionType)
            If _type IsNot Nothing Then Me.ElementExtensions.Remove(_type)
            If value Is Nothing Then
               _type = Nothing
            Else
               _type = New sss.SyndicationElementExtension(csType, Constants.Namespaces.cmisra, value)
               Me.ElementExtensions.Add(_type)
            End If
         End Set
      End Property 'Type
#End Region

      <sxs.XmlIgnore()>
      Public ReadOnly Property BaseTypeId As String
         Get
            Dim cmisraObject As Core.cmisObjectType = Me.Object
            Return If(cmisraObject Is Nothing, Nothing, cmisraObject.BaseTypeId)
         End Get
      End Property

      <sxs.XmlIgnore()>
      Public Property ContentLink As AtomLink
         Get
            If TypeOf MyBase.Content Is sss.UrlSyndicationContent Then
               With CType(MyBase.Content, sss.UrlSyndicationContent)
                  Return New AtomLink(.Url, LinkRelationshipTypes.ContentStream, .Type)
               End With
            Else
               Return Nothing
            End If
         End Get
         Private Set(value As AtomLink)
            If value Is Nothing Then
               MyBase.Content = Nothing
            Else
               MyBase.Content = New sss.UrlSyndicationContent(value.Uri, value.MediaType)
            End If
         End Set
      End Property 'ContentUri

      ''' <summary>
      ''' Atom elements take precedence over the corresponding writable CMIS property.
      ''' </summary>
      ''' <remarks>
      ''' When POSTing an Atom Document, the Atom elements MUST take precedence over the corresponding writable CMIS property. For example, atom:title will overwrite cmis:name.
      ''' This is conform to the guidelines performed in 3.5.2 Entries
      ''' </remarks>
      Public Sub EnsurePOSTRuleOfPrecedence()
         Dim cmisraObject As Core.cmisObjectType = Me.Object
         Dim cmisraType As Core.Definitions.Types.cmisTypeDefinitionType = Me.Type

         If cmisraObject IsNot Nothing Then
            Dim prop As Core.Properties.cmisProperty
            Dim propertyList As New List(Of Core.Properties.cmisProperty)
            Dim verifyProperties As New Dictionary(Of String, Core.Properties.cmisProperty)
            Dim changes As Boolean = False

            'enlist the current properties
            If cmisraObject.Properties IsNot Nothing AndAlso cmisraObject.Properties.Properties IsNot Nothing Then
               For Each prop In cmisraObject.Properties.Properties
                  If Not verifyProperties.ContainsKey(If(prop.PropertyDefinitionId, "")) Then
                     verifyProperties.Add(If(prop.PropertyDefinitionId, ""), prop)
                     propertyList.Add(prop)
                  End If
               Next
            End If

            'check, if a property is missed
            With New Common.PredefinedPropertyDefinitionFactory(Nothing)
               'atom:title MUST be the cmis:name property
               If Title IsNot Nothing Then
                  If verifyProperties.ContainsKey(CmisPredefinedPropertyNames.Name) Then
                     prop = verifyProperties(CmisPredefinedPropertyNames.Name)
                  Else
                     prop = .Name.CreateProperty()
                     verifyProperties.Add(CmisPredefinedPropertyNames.Name, prop)
                     propertyList.Add(prop)
                     changes = True
                  End If
                  prop.Value = Title.Text
               End If
               If changes Then cmisraObject.Properties = New Core.Collections.cmisPropertiesType(propertyList.ToArray())
            End With
         ElseIf cmisraType IsNot Nothing Then
            If Title IsNot Nothing Then cmisraType.DisplayName = Title.Text
         End If
      End Sub

      <sxs.XmlIgnore()>
      Public ReadOnly Property ChangeToken As String
         Get
            Dim cmisraObject As Core.cmisObjectType = Me.Object
            Return If(cmisraObject Is Nothing, Nothing, cmisraObject.ChangeToken)
         End Get
      End Property

      ''' <summary>
      ''' Return the first matching link
      ''' </summary>
      ''' <param name="relationshipType"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <sxs.XmlIgnore()>
      Public ReadOnly Property Link(relationshipType As String, Optional mediaType As String = Nothing) As sss.SyndicationLink
         Get
            For Each retVal As sss.SyndicationLink In Links
               If String.Compare(If(relationshipType, ""), If(retVal.RelationshipType, ""), True) = 0 AndAlso
                  (String.IsNullOrEmpty(mediaType) OrElse String.Compare(mediaType, If(retVal.MediaType, ""), True) = 0) Then
                  Return retVal
               End If
            Next
            'not found
            Return Nothing
         End Get
      End Property

      <sxs.XmlIgnore()>
      Public ReadOnly Property Name As String
         Get
            Dim cmisraObject As Core.cmisObjectType = Me.Object
            Return If(cmisraObject Is Nothing, Nothing, cmisraObject.Name)
         End Get
      End Property

      <sxs.XmlIgnore()>
      Public ReadOnly Property ObjectId As String
         Get
            Dim cmisraObject As Core.cmisObjectType = Me.Object
            Return If(cmisraObject Is Nothing, Nothing, cmisraObject.ObjectId)
         End Get
      End Property

      <sxs.XmlIgnore()>
      Public ReadOnly Property TypeId As String
         Get
            Dim cmisraObject As Core.cmisObjectType = Me.Object
            Dim cmisType As Core.Definitions.Types.cmisTypeDefinitionType = Me.Type

            If cmisType IsNot Nothing Then
               Return cmisType.Id
            ElseIf cmisraObject Is Nothing Then
               Return Nothing
            Else
               Return cmisraObject.ObjectTypeId
            End If
         End Get
      End Property

   End Class
End Namespace