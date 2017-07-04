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
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Extensions
   <Attributes.JavaScriptObjectResolver(GetType(JSON.Serialization.ExtensionResolver(Of Extension)), "{"""":""TExtension""}")>
   Public MustInherit Class Extension
      Inherits Serialization.XmlSerializable

#Region "Constructors"
      Shared Sub New()
         'search for all types supporting cmis typedefinition ...
         If Not ExploreAssembly(GetType(Extension).Assembly) Then
            '... failed.
            'At least register well-known Alfresco-extension-classes
            cac.ExploreTypes(Of Extension)(
               _factories, _genericTypeDefinition,
               GetType(Alfresco.Aspects),
               GetType(Alfresco.SetAspects),
               GetType(Data.ConverterDefinition),
               GetType(Data.RowCollection))
            ExploreFactories()
         End If
      End Sub
      Protected Sub New()
      End Sub
      Protected Sub New(initClassSupported As Boolean?)
         MyBase.New(initClassSupported)
      End Sub

      ''' <summary>
      ''' Creates a new instance (similar to ReadXml() in IXmlSerializable-classes)
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(reader As sx.XmlReader) As Extension
         'first chance: from current node name
         Dim nodeName As String = GetCurrentStartElementLocalName(reader)
         Dim namespacePrefix As String = GetDefaultPrefix(reader.NamespaceURI, 0)

         If Not (String.IsNullOrEmpty(nodeName) OrElse String.IsNullOrEmpty(namespacePrefix)) Then
            Dim key As String = namespacePrefix.ToLowerInvariant() & ":" & nodeName.ToLowerInvariant()
            If _factories.ContainsKey(key) Then
               Dim retVal As Extension = TryCast(_factories(key).CreateInstance(), Extension)

               If retVal IsNot Nothing Then
                  retVal.ReadXml(reader)
                  Return retVal
               End If
            End If
         End If

         'unable to interpret node as extension
         Return Nothing
      End Function

      ''' <summary>
      ''' Creates a new instance for known extensionTypeName
      ''' </summary>
      ''' <param name="extensionTypeName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(extensionTypeName As String) As Extension
         extensionTypeName = If(extensionTypeName, String.Empty).ToLowerInvariant()
         If _factories.ContainsKey(extensionTypeName) Then
            Return _factories(extensionTypeName).CreateInstance()
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Searches in assembly for types supporting extensions
      ''' </summary>
      ''' <param name="assembly"></param>
      ''' <remarks></remarks>
      Public Shared Function ExploreAssembly(assembly As System.Reflection.Assembly) As Boolean
         Try
            'explore the complete assembly if possible
            If assembly IsNot Nothing Then
               cac.ExploreTypes(Of Extension)(_factories, _genericTypeDefinition, assembly.GetTypes())
            End If
            ExploreFactories()
            Return True
         Catch
            Return False
         End Try
      End Function

      ''' <summary>
      ''' Updates GetExtensionType-support
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared Sub ExploreFactories()
         _extensionTypes.Clear()
         For Each de As KeyValuePair(Of String, CmisObjectModel.Common.Generic.Factory(Of Extension)) In _factories
            Dim factoryType As Type = de.Value.GetType()
            If factoryType.IsGenericType Then
               Dim genericArgumentTypes As Type() = de.Value.GetType().GetGenericArguments()
               If genericArgumentTypes IsNot Nothing AndAlso genericArgumentTypes.Length = 2 Then
                  Dim extensionType As Type = genericArgumentTypes(1)
                  If GetType(Extension).IsAssignableFrom(extensionType) Then _extensionTypes.Add(de.Key, genericArgumentTypes(1))
               End If
            End If
         Next
      End Sub

      ''' <summary>
      ''' Returns the identifier of the extension-class
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property ExtensionTypeName As String
         Get
            Dim attrs As Object() = Me.GetType().GetCustomAttributes(GetType(Attributes.CmisTypeInfoAttribute), False)

            If attrs IsNot Nothing AndAlso attrs.Length > 0 Then
               Return CType(attrs(0), Attributes.CmisTypeInfoAttribute).CmisTypeName
            Else
               Return String.Empty
            End If
         End Get
      End Property

      Protected Shared _extensionTypes As New Dictionary(Of String, Type)
      Protected Shared _factories As New Dictionary(Of String, Generic.Factory(Of Extension))

      ''' <summary>
      ''' Gets the extension type that declared the extensionTypeName
      ''' </summary>
      Public Shared Function GetExtensionType(extensionTypeName As String) As Type
         extensionTypeName = If(extensionTypeName, String.Empty).ToLowerInvariant()
         Return If(_extensionTypes.ContainsKey(extensionTypeName), _extensionTypes(extensionTypeName), Nothing)
      End Function

      ''' <summary>
      ''' GetType(Generic.Factory(Of Extension, TDerivedFromExtension))
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _genericTypeDefinition As Type = GetType(Generic.Factory(Of Extension, Alfresco.Aspects)).GetGenericTypeDefinition
#End Region

   End Class
End Namespace