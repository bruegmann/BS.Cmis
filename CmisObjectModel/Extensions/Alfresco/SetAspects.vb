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
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Extensions.Alfresco
   ''' <summary>
   ''' Support for Alfresco Aspect CRUD (in properties-collection)
   ''' </summary>
   ''' <remarks></remarks>
   <sxs.XmlRoot("setAspects", Namespace:=Constants.Namespaces.alf),
    Attributes.CmisTypeInfo("alf:setAspects", Nothing, "setAspects"),
    Attributes.JavaScriptConverter(GetType(JSON.Extensions.Alfresco.SetAspectsConverter))>
   Public Class SetAspects
      Inherits Extension

#Region "Constructors"
      Public Sub New()
      End Sub

      Public Sub New(ParamArray aspects As Aspect())
         _aspects = aspects
      End Sub
#End Region

#Region "Helper classes"
      Public Enum enumSetAspectsAction As Integer
         aspectsToAdd
         aspectsToRemove
      End Enum

      ''' <summary>
      ''' SetAspects-Entry
      ''' </summary>
      ''' <remarks></remarks>
      Public Class Aspect
         Inherits Aspects.Aspect

         Public Sub New(action As enumSetAspectsAction, aspectName As String)
            MyBase.New(aspectName)
            Me.Action = action
         End Sub
         Public Sub New(action As enumSetAspectsAction, aspectName As String, properties As Core.Properties.cmisProperty())
            Me.New(action, aspectName)
            If properties IsNot Nothing Then Me.Properties = New Core.Collections.cmisPropertiesType(properties)
         End Sub
         Public Sub New(action As enumSetAspectsAction, aspectName As String, properties As Core.Collections.cmisPropertiesType)
            MyBase.New(aspectName, properties)
            Me.Action = action
         End Sub

         Public ReadOnly Action As enumSetAspectsAction
      End Class
#End Region

#Region "IXmlSerializable"
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         Dim currentAspect As Aspect = Nothing
         Dim aspects As New List(Of Aspect)

         reader.MoveToContent()
         While reader.IsStartElement
            If String.Compare(reader.NamespaceURI, Namespaces.alf, True) = 0 Then
               If String.Compare(reader.LocalName, "aspectsToAdd", True) = 0 Then
                  'seal the current aspect
                  If currentAspect IsNot Nothing Then aspects.Add(currentAspect)
                  'start the next aspect
                  currentAspect = New Aspect(enumSetAspectsAction.aspectsToAdd, reader.ReadElementString())
               ElseIf String.Compare(reader.LocalName, "aspectsToRemove", True) = 0 Then
                  'seal the current aspect
                  If currentAspect IsNot Nothing Then aspects.Add(currentAspect)
                  'write the aspectsToRemove-entry
                  aspects.Add(New Aspect(enumSetAspectsAction.aspectsToRemove, reader.ReadElementString()))
                  currentAspect = Nothing
               ElseIf String.Compare(reader.LocalName, "properties", True) = 0 Then
                  'ensure aspect
                  If currentAspect Is Nothing Then currentAspect = New Aspect(enumSetAspectsAction.aspectsToAdd, Nothing)
                  'append aspects - properties
                  currentAspect.Properties = New Core.Collections.cmisPropertiesType
                  currentAspect.Properties.ReadXml(reader)
                  aspects.Add(currentAspect)
                  currentAspect.Seal()
                  currentAspect = Nothing
               End If
            End If
         End While
         'maybe there is an aspectsToAdd-Item left
         If currentAspect IsNot Nothing Then aspects.Add(currentAspect)
         _aspects = If(aspects.Count = 0, Nothing, aspects.ToArray())
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If _aspects IsNot Nothing Then
            For Each setAspect As Aspect In _aspects
               If setAspect IsNot Nothing Then
                  If Not String.IsNullOrEmpty(setAspect.AspectName) Then
                     Select Case setAspect.Action
                        Case enumSetAspectsAction.aspectsToAdd
                           WriteElement(writer, attributeOverrides, "aspectsToAdd", Namespaces.alf, setAspect.AspectName)
                        Case enumSetAspectsAction.aspectsToRemove
                           WriteElement(writer, attributeOverrides, "aspectsToRemove", Namespaces.alf, setAspect.AspectName)
                     End Select
                  End If
                  If setAspect.Properties IsNot Nothing AndAlso setAspect.Action = enumSetAspectsAction.aspectsToAdd Then
                     WriteElement(writer, attributeOverrides, "properties", Namespaces.alf, setAspect.Properties)
                  End If
               End If
            Next
         End If
      End Sub
#End Region

      Private _aspects As Aspect()
      Public Overridable Property Aspects As Aspect()
         Get
            Return _aspects
         End Get
         Set(value As Aspect())
            If _aspects IsNot value Then
               Dim oldValue As Aspect() = _aspects
               _aspects = value
               OnPropertyChanged("Aspects", value, oldValue)
            End If
         End Set
      End Property 'Aspects
   End Class
End Namespace