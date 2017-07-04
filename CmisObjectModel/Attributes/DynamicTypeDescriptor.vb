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

Namespace CmisObjectModel.Attributes
   ''' <summary>
   ''' Baseclass to generate type that depends on sourceType (that is the type the current attribute is designed for)
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class DynamicTypeDescriptor

#Region "Constructors"
      ''' <summary>
      ''' Overrides only within this class
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub New()
      End Sub

      Public Shared Function CreateInstance(staticType As Type) As DynamicTypeDescriptor
         Return New StaticTypeDescriptor(staticType)
      End Function

      ''' <summary>
      ''' To define a generic type
      ''' </summary>
      ''' <param name="genericTargetType"></param>
      ''' <param name="jsonFormattedGenericArgumentsMapping">Maps generic arguments of sourceType to
      ''' generic arguments of the genericTargetType in JSON format
      ''' i.e. {"TSourceTypeArgument3":"TTargetTypeArgument0","TSourceTypeArgument1":"TTargetTypeArgument1"}</param>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(genericTargetType As Type,
                                            jsonFormattedGenericArgumentsMapping As String) As DynamicTypeDescriptor
         If genericTargetType.IsGenericType Then
            Return New GenericTypeDescriptor(genericTargetType, jsonFormattedGenericArgumentsMapping)
         Else
            Return New StaticTypeDescriptor(genericTargetType)
         End If
      End Function
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Parameters to create a generic type
      ''' </summary>
      ''' <remarks></remarks>
      Protected Class GenericTypeDescriptor
         Inherits DynamicTypeDescriptor

         ''' <summary>
         ''' To define a generic type
         ''' </summary>
         ''' <param name="genericTargetType"></param>
         ''' <param name="jsonFormattedGenericArgumentsMapping">Maps generic arguments of sourceType to
         ''' generic arguments of the genericTargetType in JSON format
         ''' i.e. {"TSourceTypeArgument3":"TTargetTypeArgument0","TSourceTypeArgument1":"TTargetTypeArgument1"}</param>
         ''' <remarks></remarks>
         Public Sub New(genericTargetType As Type,
                        jsonFormattedGenericArgumentsMapping As String)
            Dim genericTypePositions As Dictionary(Of String, Type)
            Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
            Dim genericArgumentsMapping As Dictionary(Of String, String) =
               serializer.Deserialize(Of Dictionary(Of String, String))(jsonFormattedGenericArgumentsMapping)

            _genericTypeDefinition = genericTargetType.GetGenericTypeDefinition()
            _genericArgumentsTemplate = genericTargetType.GetGenericArguments()
            _genericArgumentsLength = _genericArgumentsTemplate.Length
            genericTypePositions = _genericTypeDefinition.GetGenericArguments().ToDictionary(Of String)(Function(currentType) currentType.Name)
            _genericArgumentsMapping = (From de As KeyValuePair(Of String, String) In genericArgumentsMapping
                                        Where Not String.IsNullOrEmpty(de.Value) AndAlso genericTypePositions.ContainsKey(de.Value)
                                        Let position As Integer = genericTypePositions(de.Value).GenericParameterPosition
                                        Select New KeyValuePair(Of String, Integer)(de.Key, position)).ToDictionary(Function(current) current.Key,
                                                                                                                    Function(current) current.Value)
         End Sub

         ''' <summary>
         ''' Creates the generic type for sourceType (that is the type the current attribute is designed for)
         ''' </summary>
         ''' <param name="sourceType"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overrides Function CreateType(sourceType As Type) As Type
            Dim typeArguments As Type() = CType(Array.CreateInstance(GetType(Type), _genericArgumentsLength), Type())
            Dim processedArgumentNames As New System.Collections.Generic.HashSet(Of String)

            'initialize with given template
            Array.Copy(_genericArgumentsTemplate, typeArguments, _genericArgumentsLength)
            'replace genericTypeArguments using _genericArgumentsMapping
            If _genericArgumentsMapping.ContainsKey(String.Empty) Then
               typeArguments(_genericArgumentsMapping(String.Empty)) = sourceType
            End If
            While sourceType IsNot GetType(Object)
               If sourceType.IsGenericType Then
                  Dim genericSourceTypeArguments As Type() = sourceType.GetGenericArguments()

                  For Each genericArgument As Type In sourceType.GetGenericTypeDefinition().GetGenericArguments()
                     Dim argumentName As String = genericArgument.Name

                     If _genericArgumentsMapping.ContainsKey(argumentName) AndAlso
                        processedArgumentNames.Add(argumentName) Then
                        typeArguments(_genericArgumentsMapping(argumentName)) =
                           genericSourceTypeArguments(genericArgument.GenericParameterPosition)
                     End If
                  Next
               End If
               sourceType = sourceType.BaseType
            End While

            Return _genericTypeDefinition.MakeGenericType(typeArguments)
         End Function

         Private ReadOnly _genericArgumentsLength As Integer
         Private ReadOnly _genericArgumentsMapping As Dictionary(Of String, Integer)
         Private ReadOnly _genericArgumentsTemplate As Type()
         Private ReadOnly _genericTypeDefinition As Type
      End Class

      ''' <summary>
      ''' Variant for static type to allow mixed usage of generic and non generic type definitions
      ''' </summary>
      ''' <remarks></remarks>
      Protected Class StaticTypeDescriptor
         Inherits DynamicTypeDescriptor

         Public Sub New(staticType As Type)
            _staticType = staticType
         End Sub

         Public Overrides Function CreateType(sourceType As System.Type) As System.Type
            Return _staticType
         End Function

         Private ReadOnly _staticType As Type
      End Class
#End Region

      Public MustOverride Function CreateType(sourceType As Type) As Type
   End Class
End Namespace