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
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Core.Choices
   Namespace Generic
      ''' <summary>
      ''' A Converter for all cmisChoice-types
      ''' </summary>
      ''' <typeparam name="TProperty"></typeparam>
      ''' <typeparam name="TChoice"></typeparam>
      ''' <remarks></remarks>
      Public Class cmisChoiceConverter(Of TProperty, TChoice As {CmisObjectModel.Core.Choices.Generic.cmisChoice(Of TProperty, TChoice), New})
         Inherits Serialization.Generic.JavaScriptConverter(Of TChoice)

#Region "Constructors"
         Public Sub New()
            MyBase.New(New Serialization.Generic.DefaultObjectResolver(Of TChoice))
         End Sub
         Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of TChoice))
            MyBase.New(objectResolver)
         End Sub
#End Region

         Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
            With context
               .Object.DisplayName = Read(.Dictionary, "displayName", .Object.DisplayName)
               .Object.Values = ReadArray(.Dictionary, "value", .Object.Values)
               .Object.Choices = ReadArray(context, "choice", .Object.Choices)
            End With
         End Sub

         Protected Overloads Overrides Sub Serialize(context As SerializationContext)
            With context
               .Add("displayName", .Object.DisplayName)
               If .Object.Values IsNot Nothing Then
                  If .Object.Values.Length = 1 Then
                     .Add("value", .Object.Values(0))
                  Else
                     .Add("value", .Object.Values)
                  End If
               End If
               If .Object.Choices IsNot Nothing Then
                  WriteArray(context, .Object.Choices, "choice")
               End If
            End With
         End Sub
      End Class
   End Namespace
End Namespace