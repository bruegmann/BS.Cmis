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
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports sc = System.ComponentModel

Namespace CmisObjectModel.Core.Definitions
   ''' <summary>
   ''' Baseclass for all Definition-Classes
   ''' (see namespaces CmisObjectModel.Core.Definitions.*)
   ''' </summary>
   ''' <remarks>This type is not directly defined in the cmis-documentation!</remarks>
   Public MustInherit Class DefinitionBase
      Inherits Serialization.XmlSerializable

#Region "Constructors"
      Shared Sub New()
         'search for all types supporting cmis typedefinition ...
         If Not ExploreAssembly(GetType(DefinitionBase).Assembly) Then
            '... failed.
            'At least register well-known TypeDefinition-classes ...
            cac.ExploreTypes(Of DefinitionBase)(
               _factories, _genericTypeDefinition,
               GetType(Types.cmisTypeDocumentDefinitionType),
               GetType(Types.cmisTypeFolderDefinitionType),
               GetType(Types.cmisTypeItemDefinitionType),
               GetType(Types.cmisTypePolicyDefinitionType),
               GetType(Types.cmisTypeRelationshipDefinitionType),
               GetType(Types.cmisTypeRM_ClientMgtRetentionDefinitionType),
               GetType(Types.cmisTypeRM_DestructionRetentionDefinitionType),
               GetType(Types.cmisTypeRM_HoldDefinitionType),
               GetType(Types.cmisTypeRM_RepMgtRetentionDefinitionType),
               GetType(Types.cmisTypeSecondaryDefinitionType))
            '... and PropertyDetinition-classes
            cac.ExploreTypes(Of DefinitionBase)(
               _factories, _genericTypeDefinition,
               GetType(Properties.cmisPropertyBooleanDefinitionType),
               GetType(Properties.cmisPropertyDateTimeDefinitionType),
               GetType(Properties.cmisPropertyDecimalDefinitionType),
               GetType(Properties.cmisPropertyHtmlDefinitionType),
               GetType(Properties.cmisPropertyIdDefinitionType),
               GetType(Properties.cmisPropertyIntegerDefinitionType),
               GetType(Properties.cmisPropertyStringDefinitionType),
               GetType(Properties.cmisPropertyUriDefinitionType))
         End If

         AddHandler Common.DecimalRepresentationChanged, AddressOf OnDecimalRepresentationChanged
         If Common.DecimalRepresentation <> enumDecimalRepresentation.decimal Then
            OnDecimalRepresentationChanged(Common.DecimalRepresentation)
         End If
      End Sub
      Protected Sub New()
         InitClass()
      End Sub
      ''' <summary>
      ''' this constructor is only used if derived classes from this class needs an InitClass()-call
      ''' </summary>
      ''' <param name="initClassSupported"></param>
      ''' <remarks></remarks>
      Protected Sub New(initClassSupported As Boolean?)
         MyBase.New(initClassSupported)
      End Sub
      Protected Sub New(id As String, localName As String, displayName As String, queryName As String)
         InitClass()
         _id = id
         _localName = localName
         _queryName = queryName
         _displayName = displayName
      End Sub
      Protected Sub New(id As String, localName As String, localNamespace As String, displayName As String, queryName As String, queryable As Boolean)
         InitClass()
         _id = id
         _localName = localName
         _localNamespace = localNamespace
         _displayName = displayName
         _queryName = queryName
         _queryable = queryable
      End Sub
      Protected Sub New(id As String, localName As String, localNamespace As String, displayName As String, queryName As String,
                        description As String, queryable As Boolean)
         InitClass()
         _id = id
         _localName = localName
         _localNamespace = localNamespace
         _displayName = displayName
         _queryName = queryName
         _description = description
         _queryable = queryable
      End Sub
      ''' <summary>
      ''' Creates a BaseDefinition-instance from the current node of the reader-object using the
      ''' elementName-Node to determine, which implementation is to be used
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="elementName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Shared Function CreateInstance(Of TResult As DefinitionBase)(reader As System.Xml.XmlReader,
                                                                             elementName As String) As TResult
         Dim outerXml As String

         Try
            'read the complete node to determine the required PropertyDefinition-type to allow random access
            reader.MoveToContent()
            outerXml = reader.ReadOuterXml()

            If outerXml <> "" Then
               Dim xmlDoc As New System.Xml.XmlDocument

               xmlDoc.LoadXml(outerXml)
               For Each node As System.Xml.XmlNode In xmlDoc.DocumentElement.ChildNodes
                  If String.Compare(elementName, node.LocalName, True) = 0 Then
                     Dim retVal As TResult = CreateInstance(Of TResult)(node.InnerText)

                     If retVal IsNot Nothing Then
                        Using ms As New System.IO.MemoryStream
                           xmlDoc.Save(ms)
                           ms.Position = 0
                           reader = System.Xml.XmlReader.Create(ms)
                           retVal.ReadXml(reader)
                        End Using

                        Return retVal
                     End If
                  End If
               Next
            End If
         Catch
         End Try

         'current node doesn't describe a TResult-instance
         Return Nothing
      End Function
      Protected Shared Function CreateInstance(Of TResult As DefinitionBase)(key As String) As TResult
         key = If(key Is Nothing, "", key.ToLowerInvariant())
         If _factories.ContainsKey(key) Then
            Return TryCast(_factories(key).CreateInstance(), TResult)
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Searches in assembly for types supporting cmis typedefinition
      ''' </summary>
      ''' <param name="assembly"></param>
      ''' <remarks></remarks>
      Public Shared Function ExploreAssembly(assembly As System.Reflection.Assembly) As Boolean
         Try
            'explore the complete assembly if possible
            If assembly IsNot Nothing Then
               cac.ExploreTypes(Of DefinitionBase)(_factories, _genericTypeDefinition, assembly.GetTypes())
            End If
            Return True
         Catch
            Return False
         End Try
      End Function

      Protected Shared _factories As New Dictionary(Of String, Generic.Factory(Of DefinitionBase))
      ''' <summary>
      ''' GetType(Generic.Factory(Of DefinitionBase, TDerivedFromDefinitionBase))
      ''' </summary>
      ''' <remarks></remarks>
      Protected Shared _genericTypeDefinition As Type = GetType(Generic.Factory(Of DefinitionBase, Properties.cmisPropertyBooleanDefinitionType)).GetGenericTypeDefinition
#End Region

#Region "base property-set of definition-types"
      Protected _description As String
      Public Property Description As String
         Get
            Return _description
         End Get
         Set(value As String)
            If _description <> value Then
               Dim oldValue As String = _description
               _description = value
               OnPropertyChanged("Description", value, oldValue)
            End If
         End Set
      End Property

      Protected _displayName As String
      Public Property DisplayName As String
         Get
            Return _displayName
         End Get
         Set(value As String)
            If _displayName <> value Then
               Dim oldValue As String = _displayName
               _displayName = value
               OnPropertyChanged("DisplayName", value, oldValue)
            End If
         End Set
      End Property

      Protected _id As String
      Public Property Id As String
         Get
            Return _id
         End Get
         Set(value As String)
            If _id <> value Then
               Dim oldValue As String = _id
               _id = value
               OnPropertyChanged("Id", value, oldValue)
            End If
         End Set
      End Property

      Protected _localName As String
      Public Property LocalName As String
         Get
            Return _localName
         End Get
         Set(value As String)
            If _localName <> value Then
               Dim oldValue As String = _localName
               _localName = value
               OnPropertyChanged("LocalName", value, oldValue)
            End If
         End Set
      End Property

      Protected _localNamespace As String
      Public Property LocalNamespace As String
         Get
            Return _localNamespace
         End Get
         Set(value As String)
            If _localNamespace <> value Then
               Dim oldValue As String = _localNamespace
               _localNamespace = value
               OnPropertyChanged("LocalNamespace", value, oldValue)
            End If
         End Set
      End Property

      Protected _queryable As Boolean
      Public Property Queryable As Boolean
         Get
            Return _queryable
         End Get
         Set(value As Boolean)
            If _queryable <> value Then
               Dim oldValue As Boolean = _queryable
               _queryable = value
               OnPropertyChanged("Queryable", value, oldValue)
            End If
         End Set
      End Property

      Protected _queryName As String
      Public Property QueryName As String
         Get
            Return _queryName
         End Get
         Set(value As String)
            If _queryName <> value Then
               Dim oldValue As String = _queryName
               _queryName = value.ValidateQueryName()
               OnPropertyChanged("QueryName", _queryName, oldValue)
            End If
         End Set
      End Property
#End Region

      Private Shared Sub OnDecimalRepresentationChanged(newValue As enumDecimalRepresentation)
         Dim attrs As Object() = GetType(Properties.cmisPropertyDecimalDefinitionType).GetCustomAttributes(GetType(cac), False)
         Dim attr As cac = If(attrs IsNot Nothing AndAlso attrs.Length > 0, CType(attrs(0), cac), Nothing)

         Select Case newValue
            Case enumDecimalRepresentation.decimal
               cac.AppendTypeFactory(Of DefinitionBase)(_factories, _genericTypeDefinition, GetType(Properties.cmisPropertyDecimalDefinitionType), attr)
            Case enumDecimalRepresentation.double
               cac.AppendTypeFactory(Of DefinitionBase)(_factories, _genericTypeDefinition, GetType(Properties.cmisPropertyDoubleDefinitionType), attr)
         End Select
      End Sub

   End Class
End Namespace