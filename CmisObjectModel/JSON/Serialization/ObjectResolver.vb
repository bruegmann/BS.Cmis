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
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Serialization
   ''' <summary>
   ''' Baseclass of ObjectResolver-instances
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class ObjectResolver
      Public Function CreateInstance(dictionary As IDictionary(Of String, Object)) As Object
         Return CreateInstanceCore(dictionary)
      End Function
      Protected MustOverride Function CreateInstanceCore(source As Object) As Object
   End Class

   ''' <summary>
   ''' Objectresolver for Core.Properties.cmisProperty types
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisPropertyResolver
      Inherits Generic.ObjectResolver(Of CmisObjectModel.Core.Properties.cmisProperty)

      Public Overrides Function CreateInstance(source As Object) As CmisObjectModel.Core.Properties.cmisProperty
         If TypeOf source Is System.Collections.Generic.IDictionary(Of String, Object) Then
            Dim dictionary As System.Collections.Generic.IDictionary(Of String, Object) =
               CType(source, System.Collections.Generic.IDictionary(Of String, Object))
            Dim typeName As String = If(dictionary.ContainsKey("type"), CStr(dictionary("type")), "string")
            Dim type As CmisObjectModel.Core.enumPropertyType

            Select Case If(TryParse(typeName, type, True), type, CmisObjectModel.Core.enumPropertyType.string)
               Case CmisObjectModel.Core.enumPropertyType.boolean
                  Return New CmisObjectModel.Core.Properties.cmisPropertyBoolean()
               Case CmisObjectModel.Core.enumPropertyType.datetime
                  Return New CmisObjectModel.Core.Properties.cmisPropertyDateTime()
               Case CmisObjectModel.Core.enumPropertyType.decimal
                  If Common.DecimalRepresentation = enumDecimalRepresentation.decimal Then
                     Return New CmisObjectModel.Core.Properties.cmisPropertyDecimal()
                  Else
                     Return New CmisObjectModel.Core.Properties.cmisPropertyDouble()
                  End If
               Case CmisObjectModel.Core.enumPropertyType.html
                  Return New CmisObjectModel.Core.Properties.cmisPropertyHtml()
               Case CmisObjectModel.Core.enumPropertyType.id
                  Return New CmisObjectModel.Core.Properties.cmisPropertyId()
               Case CmisObjectModel.Core.enumPropertyType.integer
                  Return New CmisObjectModel.Core.Properties.cmisPropertyInteger()
               Case CmisObjectModel.Core.enumPropertyType.string
                  Return New CmisObjectModel.Core.Properties.cmisPropertyString()
               Case CmisObjectModel.Core.enumPropertyType.uri
                  Return New CmisObjectModel.Core.Properties.cmisPropertyUri()
               Case Else
                  Return New CmisObjectModel.Core.Properties.cmisPropertyString()
            End Select
         ElseIf source Is Nothing Then
            Return New CmisObjectModel.Core.Properties.cmisPropertyObject()
         Else
            Return CmisObjectModel.Core.Properties.cmisProperty.FromType(source.GetType())
         End If
      End Function
   End Class

   ''' <summary>
   ''' Objectresolver for Core.Definitions.Properties.cmisPropertyDefinitionType types
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisPropertyDefinitionResolver
      Inherits Generic.ObjectResolver(Of CmisObjectModel.Core.Definitions.Properties.cmisPropertyDefinitionType)

      Public Overrides Function CreateInstance(source As Object) As CmisObjectModel.Core.Definitions.Properties.cmisPropertyDefinitionType
         If TypeOf source Is System.Collections.Generic.IDictionary(Of String, Object) Then
            Dim dictionary As System.Collections.Generic.IDictionary(Of String, Object) =
               CType(source, System.Collections.Generic.IDictionary(Of String, Object))
            Dim typeName As String = If(dictionary.ContainsKey("propertyType"), CStr(dictionary("propertyType")), "string")
            Dim type As CmisObjectModel.Core.enumPropertyType

            Select Case If(TryParse(typeName, type, True), type, CmisObjectModel.Core.enumPropertyType.string)
               Case CmisObjectModel.Core.enumPropertyType.boolean
                  Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyBooleanDefinitionType()
               Case CmisObjectModel.Core.enumPropertyType.datetime
                  Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyDateTimeDefinitionType
               Case CmisObjectModel.Core.enumPropertyType.decimal
                  If Common.DecimalRepresentation = enumDecimalRepresentation.decimal Then
                     Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyDecimalDefinitionType()
                  Else
                     Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyDoubleDefinitionType()
                  End If
               Case CmisObjectModel.Core.enumPropertyType.html
                  Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyHtmlDefinitionType()
               Case CmisObjectModel.Core.enumPropertyType.id
                  Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyIdDefinitionType()
               Case CmisObjectModel.Core.enumPropertyType.integer
                  Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyIntegerDefinitionType()
               Case CmisObjectModel.Core.enumPropertyType.string
                  Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyStringDefinitionType()
               Case CmisObjectModel.Core.enumPropertyType.uri
                  Return New CmisObjectModel.Core.Definitions.Properties.cmisPropertyUriDefinitionType
            End Select
         End If

         Return Nothing
      End Function
   End Class

   ''' <summary>
   ''' Objectresolver for Core.Definitions.Types.cmisTypeDefinitionType types
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisTypeDefinitionResolver
      Inherits Generic.ObjectResolver(Of CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType)

      Public Overrides Function CreateInstance(source As Object) As CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType
         If TypeOf source Is System.Collections.Generic.IDictionary(Of String, Object) Then
            Dim dictionary As System.Collections.Generic.IDictionary(Of String, Object) =
               CType(source, System.Collections.Generic.IDictionary(Of String, Object))
            Dim baseIdName As String = If(dictionary.ContainsKey("baseId"), CStr(dictionary("baseId")),
                                          CmisObjectModel.Core.enumBaseObjectTypeIds.cmisDocument.GetName())
            Dim baseId As CmisObjectModel.Core.enumBaseObjectTypeIds

            Select Case If(TryParse(baseIdName, baseId, True), baseId, CmisObjectModel.Core.enumBaseObjectTypeIds.cmisDocument)
               Case CmisObjectModel.Core.enumBaseObjectTypeIds.cmisDocument
                  Return New CmisObjectModel.Core.Definitions.Types.cmisTypeDocumentDefinitionType()
               Case CmisObjectModel.Core.enumBaseObjectTypeIds.cmisFolder
                  Return New CmisObjectModel.Core.Definitions.Types.cmisTypeFolderDefinitionType()
               Case CmisObjectModel.Core.enumBaseObjectTypeIds.cmisItem
                  Return New CmisObjectModel.Core.Definitions.Types.cmisTypeItemDefinitionType()
               Case CmisObjectModel.Core.enumBaseObjectTypeIds.cmisPolicy
                  Return New CmisObjectModel.Core.Definitions.Types.cmisTypePolicyDefinitionType()
               Case CmisObjectModel.Core.enumBaseObjectTypeIds.cmisRelationship
                  Return New CmisObjectModel.Core.Definitions.Types.cmisTypeRelationshipDefinitionType()
               Case CmisObjectModel.Core.enumBaseObjectTypeIds.cmisSecondary
                  Dim id As String = If(dictionary.ContainsKey("id"), CStr(dictionary("id")),
                                        CmisObjectModel.Core.Definitions.Types.cmisTypeSecondaryDefinitionType.TargetTypeName)
                  Select Case True
                     Case String.Compare(id, CmisObjectModel.Core.Definitions.Types.cmisTypeSecondaryDefinitionType.TargetTypeName, True) = 0
                        Return New CmisObjectModel.Core.Definitions.Types.cmisTypeSecondaryDefinitionType()
                     Case String.Compare(id, CmisObjectModel.Core.Definitions.Types.cmisTypeRM_ClientMgtRetentionDefinitionType.TargetTypeName, True) = 0
                        Return New CmisObjectModel.Core.Definitions.Types.cmisTypeRM_ClientMgtRetentionDefinitionType()
                     Case String.Compare(id, CmisObjectModel.Core.Definitions.Types.cmisTypeRM_DestructionRetentionDefinitionType.TargetTypeName, True) = 0
                        Return New CmisObjectModel.Core.Definitions.Types.cmisTypeRM_DestructionRetentionDefinitionType()
                     Case String.Compare(id, CmisObjectModel.Core.Definitions.Types.cmisTypeRM_HoldDefinitionType.TargetTypeName, True) = 0
                        Return New CmisObjectModel.Core.Definitions.Types.cmisTypeRM_HoldDefinitionType()
                     Case String.Compare(id, CmisObjectModel.Core.Definitions.Types.cmisTypeRM_RepMgtRetentionDefinitionType.TargetTypeName, True) = 0
                        Return New CmisObjectModel.Core.Definitions.Types.cmisTypeRM_RepMgtRetentionDefinitionType()
                     Case Else
                        Return New CmisObjectModel.Core.Definitions.Types.cmisTypeSecondaryDefinitionType()
                  End Select
            End Select
         End If

         Return Nothing
      End Function
   End Class

   ''' <summary>
   ''' ObjectResolver for Extensions.Extension types
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ExtensionResolver(Of TExtension As CmisObjectModel.Extensions.Extension)
      Inherits Generic.ObjectResolver(Of TExtension)

      Public Overrides Function CreateInstance(source As Object) As TExtension
         If TypeOf source Is System.Collections.Generic.IDictionary(Of String, Object) Then
            Dim dictionary As System.Collections.Generic.IDictionary(Of String, Object) =
               CType(source, System.Collections.Generic.IDictionary(Of String, Object))
            'try quick 
            If dictionary.ContainsKey("extensionTypeName") Then
               Return TryCast(CmisObjectModel.Extensions.Extension.CreateInstance(CStr(dictionary("extensionTypeName"))), TExtension)
            ElseIf dictionary.ContainsKey("ExtensionTypeName") Then
               Return TryCast(CmisObjectModel.Extensions.Extension.CreateInstance(CStr(dictionary("ExtensionTypeName"))), TExtension)
            Else
               'try slow
               For Each de As KeyValuePair(Of String, Object) In dictionary
                  If String.Compare(de.Key, "extensionTypeName", True) = 0 Then
                     Return TryCast(CmisObjectModel.Extensions.Extension.CreateInstance(CStr(de.Value)), TExtension)
                  End If
               Next
            End If
         End If
         Return Nothing
      End Function
   End Class

   Namespace Generic
      ''' <summary>
      ''' Objects with default constructor
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <remarks></remarks>
      Public Class DefaultObjectResolver(Of T As New)
         Inherits ObjectResolver(Of T)

         Public Overrides Function CreateInstance(source As Object) As T
            Return New T()
         End Function
      End Class

      ''' <summary>
      ''' Objects with default constructor of derived class
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <remarks></remarks>
      Public Class DefaultObjectResolver(Of TBase, T As {New, TBase})
         Inherits ObjectResolver(Of TBase)

         Public Overrides Function CreateInstance(source As Object) As TBase
            Return New T()
         End Function
      End Class

      Public MustInherit Class ObjectResolver(Of T)
         Inherits ObjectResolver

         Public MustOverride Shadows Function CreateInstance(source As Object) As T
         Protected NotOverridable Overrides Function CreateInstanceCore(source As Object) As Object
            Return CreateInstance(source)
         End Function
      End Class
   End Namespace
End Namespace