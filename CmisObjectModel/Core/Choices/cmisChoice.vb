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
Imports ccg = CmisObjectModel.Collections.Generic
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Core.Choices
   <sxs.XmlRoot("choice", Namespace:=Constants.Namespaces.cmis)>
   Partial Public Class cmisChoice

      Protected Sub New(displayName As String)
         _displayName = displayName
      End Sub

      Public Property Choices As cmisChoice()
         Get
            Return ChoicesCore
         End Get
         Set(value As cmisChoice())
            ChoicesCore = value
         End Set
      End Property
      Protected MustOverride Property ChoicesCore As cmisChoice()

      Private _choicesAsReadOnly As New ccg.ArrayMapper(Of cmisChoice, cmisChoice)(Me,
                                                                                   "Choices", Function() ChoicesCore,
                                                                                   "DisplayName", Function(choice) choice.DisplayName)
      ''' <summary>
      ''' Access to choices via index or DisplayName
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property ChoicesAsReadOnly As ccg.ArrayMapper(Of cmisChoice, cmisChoice)
         Get
            Return _choicesAsReadOnly
         End Get
      End Property

      ''' <summary>
      ''' Create new child
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public MustOverride Function CreateChild() As cmisChoice

      Public Property Values As Object()
         Get
            Return ValuesCore
         End Get
         Set(value As Object())
            ValuesCore = value
         End Set
      End Property
      Protected MustOverride Property ValuesCore As Object()

   End Class

   Namespace Generic
      ''' <summary>
      ''' Generic Version of cmisChoice
      ''' </summary>
      ''' <typeparam name="TProperty"></typeparam>
      ''' <typeparam name="TDerived"></typeparam>
      ''' <remarks>Baseclass for all typesafe cmisChoice-classes</remarks>
      <Attributes.JavaScriptConverter(GetType(JSON.Core.Choices.Generic.cmisChoiceConverter(Of Boolean, cmisChoiceBoolean)),
                                      "{""TProperty"":""TProperty"",""TDerived"":""TChoice""}")>
      Public MustInherit Class cmisChoice(Of TProperty, TDerived As {cmisChoice(Of TProperty, TDerived), New})
         Inherits cmisChoice

         Protected Sub New()
            MyBase.New()
         End Sub
         ''' <summary>
         ''' this constructor is only used if derived classes from this class needs an InitClass()-call
         ''' </summary>
         ''' <param name="initClassSupported"></param>
         ''' <remarks></remarks>
         Protected Sub New(initClassSupported As Boolean?)
            MyBase.New(initClassSupported)
         End Sub
         Protected Sub New(displayName As String, ParamArray choices As TDerived())
            MyBase.New(displayName)
         End Sub
         Protected Sub New(displayName As String, values As TProperty(), ParamArray choices As TDerived())
            MyBase.New(displayName)
         End Sub

         Protected _choices As TDerived()
         Public Overridable Shadows Property Choices As TDerived()
            Get
               Return _choices
            End Get
            Set(value As TDerived())
               If value IsNot _choices Then
                  Dim oldValue As TDerived() = _choices
                  _choices = value
                  OnPropertyChanged("Choices", value, oldValue)
               End If
            End Set
         End Property 'Choices
         Protected Overrides Property ChoicesCore As cmisChoice()
            Get
               If _choices Is Nothing Then
                  Return Nothing
               Else
                  Return (From choice As cmisChoice In _choices
                          Select choice).ToArray()
               End If
            End Get
            Set(value As cmisChoice())
               If value Is Nothing Then
                  Choices = Nothing
               Else
                  Choices = (From choice As cmisChoice In value
                             Where TypeOf choice Is TDerived
                             Select CType(choice, TDerived)).ToArray()
               End If
            End Set
         End Property

         Private _choicesAsReadOnly As New ccg.ArrayMapper(Of cmisChoice, TDerived)(Me,
                                                                                    "Choices", Function() _choices,
                                                                                    "DisplayName", Function(choice) choice.DisplayName)
         ''' <summary>
         ''' Access to choices via index or DisplayName
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Shadows ReadOnly Property ChoicesAsReadOnly As ccg.ArrayMapper(Of cmisChoice, TDerived)
            Get
               Return _choicesAsReadOnly
            End Get
         End Property

         Public Overrides Function CreateChild() As cmisChoice
            Return New TDerived()
         End Function

         Protected _values As TProperty()
         Public Overridable Shadows Property Values As TProperty()
            Get
               Return _values
            End Get
            Set(value As TProperty())
               If value IsNot _values Then
                  Dim oldValue As TProperty() = _values
                  _values = value
                  OnPropertyChanged("Values", value, oldValue)
               End If
            End Set
         End Property 'Values
         Protected Overrides Property ValuesCore As Object()
            Get
               If _values Is Nothing Then
                  Return Nothing
               Else
                  Return (From item As Object In _values
                          Select item).ToArray()
               End If
            End Get
            Set(value As Object())
               If value Is Nothing Then
                  Values = Nothing
               Else
                  Values = (From item As Object In value
                            Where TypeOf item Is TProperty
                            Select CType(item, TProperty)).ToArray()
               End If
            End Set
         End Property

      End Class
   End Namespace
End Namespace