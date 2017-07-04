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

Namespace CmisObjectModel.Attributes
   ''' <summary>
   ''' Class contains the CmisTypeName of a class, the TargetTypeName that is
   ''' the type this class is designed for, and the DefaultElementName that is
   ''' the node name within a xml-file, in which this class is presented
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisTypeInfoAttribute
      Inherits Attribute

      Public Sub New(cmisTypeName As String, targetTypeName As String, defaultElementName As String)
         Me.CmisTypeName = If(cmisTypeName Is Nothing, "", cmisTypeName)
         Me.TargetTypeName = If(targetTypeName Is Nothing, "", targetTypeName)
         Me.DefaultElementName = If(defaultElementName Is Nothing, "", defaultElementName)
      End Sub

      Public ReadOnly CmisTypeName As String
      Public ReadOnly DefaultElementName As String
      Public ReadOnly TargetTypeName As String

#Region "Explore environment"
      ''' <summary>
      ''' Searches for types derived from TBase, constructible by default constructor and
      ''' supporting the CmisTypeInfoAttribute
      ''' </summary>
      ''' <typeparam name="TBase"></typeparam>
      ''' <param name="factories"></param>
      ''' <param name="genericTypeDefinition"></param>
      ''' <param name="types"></param>
      ''' <remarks></remarks>
      Public Shared Sub ExploreTypes(Of TBase As Serialization.XmlSerializable)(factories As Dictionary(Of String, Generic.Factory(Of TBase)),
                                                                                genericTypeDefinition As Type,
                                                                                ParamArray types As Type())
         Dim baseType As Type = GetType(TBase)

         SyncLock factories
            If types IsNot Nothing Then
               For Each type As Type In types
                  Dim attrs As Object() = type.GetCustomAttributes(GetType(CmisTypeInfoAttribute), False)

                  If attrs IsNot Nothing AndAlso attrs.Length > 0 Then
                     AppendTypeFactory(Of TBase)(factories, genericTypeDefinition, type, baseType, CType(attrs(0), CmisTypeInfoAttribute))
                  End If
               Next
            End If
         End SyncLock
      End Sub

      Public Shared Sub AppendTypeFactory(Of TBase As Serialization.XmlSerializable)(factories As Dictionary(Of String, Generic.Factory(Of TBase)),
                                                                                     genericTypeDefinition As Type,
                                                                                     type As Type, attr As CmisTypeInfoAttribute)
         If attr IsNot Nothing Then AppendTypeFactory(Of TBase)(factories, genericTypeDefinition, type, GetType(TBase), attr)
      End Sub

      Protected Shared Sub AppendTypeFactory(Of TBase As Serialization.XmlSerializable)(factories As Dictionary(Of String, Generic.Factory(Of TBase)),
                                                                                        genericTypeDefinition As Type, type As Type,
                                                                                        baseType As Type, attr As CmisTypeInfoAttribute)
         Try
            If baseType.IsAssignableFrom(type) AndAlso Not type.IsAbstract AndAlso
               type.GetConstructor(New Type() {}) IsNot Nothing Then
               'create factory for this valid type
               Dim factory As Generic.Factory(Of TBase) =
                  CType(System.Activator.CreateInstance(genericTypeDefinition.MakeGenericType(baseType, type)), Generic.Factory(Of TBase))
               Dim cmisFullTypeName As String
               Dim cmisTypeName As String
               Dim targetTypeName As String
               Dim defaultElementName As String

               With attr
                  cmisFullTypeName = .CmisTypeName.ToLowerInvariant()
                  cmisTypeName = If(Not String.IsNullOrEmpty(cmisFullTypeName), cmisFullTypeName.Substring(cmisFullTypeName.IndexOf(":"c) + 1), "")
                  targetTypeName = .TargetTypeName.ToLowerInvariant()
                  defaultElementName = .DefaultElementName.ToLowerInvariant()
               End With

               'provide type construction by cmisFullTypeName, cmisTypeName, targetTypeName and defaultElementName
               For Each key As String In New String() {cmisFullTypeName, cmisTypeName, targetTypeName, defaultElementName}
                  If factories.ContainsKey(key) Then
                     factories(key) = factory
                  ElseIf key <> "" Then
                     factories.Add(key, factory)
                  End If
               Next
            End If
         Catch
         End Try
      End Sub
#End Region

   End Class
End Namespace