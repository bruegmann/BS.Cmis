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
Imports ca = CmisObjectModel.AtomPub
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports cce = CmisObjectModel.Constants.ExtendedProperties
Imports ccg = CmisObjectModel.Client.Generic
Imports ce = CmisObjectModel.Extensions
Imports cea = CmisObjectModel.Extensions.Alfresco
Imports cmr = CmisObjectModel.Messaging.Requests
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Client.Vendors
   ''' <summary>
   ''' Support for Alfresco extensions like aspects
   ''' </summary>
   ''' <remarks></remarks>
   Public Class Alfresco
      Inherits Vendor

#Region "Constructors"
      ''' <summary>
      ''' First key: repositoryId, second key: aspect identifier
      ''' </summary>
      ''' <remarks></remarks>
      Private _aspects As New Dictionary(Of String, Dictionary(Of String, ccdt.cmisTypeDefinitionType))
      Public Sub New(client As Contracts.ICmisClient)
         MyBase.New(client)
      End Sub
#End Region

      ''' <summary>
      ''' Reads the defined aspects in an Alfresco repository
      ''' </summary>
      ''' <remarks></remarks>
      Private Function GetAspects(repositoryId As String) As Dictionary(Of String, ccdt.cmisTypeDefinitionType)
         SyncLock _aspects
            If _aspects.ContainsKey(repositoryId) Then
               Return _aspects(repositoryId)
            Else
               'in Alfresco aspect types are derived from policy type 'P:cmisext:aspects'
               Dim retVal As Dictionary(Of String, ccdt.cmisTypeDefinitionType) =
                  GetTypeDescendants(repositoryId, "P:cmisext:aspects")
               _aspects.Add(repositoryId, retVal)

               Return retVal
            End If
         End SyncLock
      End Function

      Private _propertyFilter As New HashSet(Of String) From {"cm", "exif"}
      ''' <summary>
      ''' Collects all propertyDefinition of the secondary types (aspects)
      ''' </summary>
      ''' <param name="state"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overrides Function GetSecondaryTypeProperties(state As State) As System.Collections.Generic.Dictionary(Of String, Core.Definitions.Properties.cmisPropertyDefinitionType)
         Return AddSecondaryProperties(MyBase.GetSecondaryTypeProperties(state),
                                       GetAspects(state.RepositoryId), state.SecondaryTypeIds, _propertyFilter)
      End Function

      Protected Overrides Function BeginRequest(state As State, properties As Core.Collections.cmisPropertiesType) As State
         Dim currentProperties = If(properties Is Nothing, Nothing, properties.Properties)

         If currentProperties IsNot Nothing Then
            Dim setAspects As New Dictionary(Of String, List(Of Core.Properties.cmisProperty))
            Dim aspects As Dictionary(Of String, ccdt.cmisTypeDefinitionType) = GetAspects(state.RepositoryId)
            Dim objectProperties As New List(Of Core.Properties.cmisProperty)

            'find aspects if supported (since alfresco 4.2d the repository delivers aspects as secondaryObjectTypes)
            If aspects.Count > 0 Then
               For Each cmisProperty As Core.Properties.cmisProperty In currentProperties
                  Dim extendedProperties As Dictionary(Of String, Object) = If(cmisProperty Is Nothing, Nothing, cmisProperty.ExtendedProperties(False))
                  Dim declaringType = If(extendedProperties Is Nothing OrElse Not extendedProperties.ContainsKey(cce.DeclaringType),
                                         Nothing, CStr(extendedProperties(cce.DeclaringType)))
                  If Not String.IsNullOrEmpty(declaringType) AndAlso aspects.ContainsKey(declaringType) Then
                     'aspectToAdd
                     Dim aspectProperties As List(Of Core.Properties.cmisProperty)

                     If setAspects.ContainsKey(declaringType) Then
                        aspectProperties = setAspects(declaringType)
                     Else
                        aspectProperties = New List(Of Core.Properties.cmisProperty)
                        setAspects.Add(declaringType, aspectProperties)
                     End If
                     aspectProperties.Add(cmisProperty)
                  Else
                     objectProperties.Add(cmisProperty)
                  End If
               Next
               'aspects defined
               If setAspects.Count > 0 Then
                  Dim currentExtensions = properties.Extensions
                  Dim extensions As List(Of Extensions.Extension)

                  If currentExtensions Is Nothing Then
                     extensions = New List(Of Extensions.Extension)
                  Else
                     extensions = (From extension As Extensions.Extension In currentExtensions
                                   Where Not (extension Is Nothing OrElse TypeOf extension Is Extensions.Alfresco.Aspects OrElse
                                              TypeOf extension Is Extensions.Alfresco.SetAspects)
                                   Select extension).ToList
                  End If
                  'aspect-properties must be removed from property collection ...
                  properties.Properties = objectProperties.ToArray()
                  '... and send as SetAspects extension
                  extensions.Add(New Extensions.Alfresco.SetAspects((From de As KeyValuePair(Of String, List(Of Core.Properties.cmisProperty)) In setAspects
                                                                     Select New Extensions.Alfresco.SetAspects.Aspect(CmisObjectModel.Extensions.Alfresco.SetAspects.enumSetAspectsAction.aspectsToAdd,
                                                                                                                      de.Key, de.Value.ToArray())).ToArray))
                  properties.Extensions = extensions.ToArray()
                  'after the properties have been sent to the server the client has to restore modified values
                  state.AddRollbackAction(Sub()
                                             properties.Extensions = currentExtensions
                                             properties.Properties = currentProperties
                                          End Sub)
               End If
            End If
         End If

         Return MyBase.BeginRequest(state, properties)
      End Function

      Public Overrides Sub EndRequest(state As State, ParamArray propertyCollections As Core.Collections.cmisPropertiesType())
         MyBase.EndRequest(state, propertyCollections)

         If propertyCollections IsNot Nothing Then
            For Each propertyCollection As Core.Collections.cmisPropertiesType In propertyCollections
               If propertyCollection IsNot Nothing AndAlso propertyCollection.Extensions IsNot Nothing Then
                  Dim properties As Dictionary(Of String, Core.Properties.cmisProperty) = propertyCollection.GetProperties(True)
                  Dim hasAspects As Boolean = False

                  For Each extension As Extensions.Extension In propertyCollection.Extensions
                     If TypeOf extension Is Extensions.Alfresco.Aspects Then
                        Dim aspects As Extensions.Alfresco.Aspects = CType(extension, Extensions.Alfresco.Aspects)
                        If aspects.AppliedAspects IsNot Nothing Then
                           For Each aspect As Extensions.Alfresco.Aspects.Aspect In aspects.AppliedAspects
                              If aspect IsNot Nothing AndAlso aspect.Properties IsNot Nothing AndAlso aspect.Properties.Properties IsNot Nothing Then
                                 For Each cmisProperty As Core.Properties.cmisProperty In aspect.Properties.Properties
                                    If cmisProperty IsNot Nothing AndAlso Not properties.ContainsKey(cmisProperty.PropertyDefinitionId) Then
                                       properties.Add(cmisProperty.PropertyDefinitionId, cmisProperty)
                                       hasAspects = True
                                    End If
                                 Next
                              End If
                           Next
                        End If
                     End If
                  Next
                  If hasAspects Then propertyCollection.Properties = properties.Values.ToArray()
               End If
            Next
         End If
      End Sub
   End Class
End Namespace