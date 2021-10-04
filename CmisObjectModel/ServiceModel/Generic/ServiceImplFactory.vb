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
Imports sr = System.Reflection
Imports sre = System.Reflection.Emit

Namespace CmisObjectModel.ServiceModel.Generic
   ''' <summary>
   ''' Implements mechanism to create a TServiceImpl-instance to handle all requests
   ''' of this cmis framework
   ''' </summary>
   ''' <remarks>
   ''' Creates TServiceImpl when needed. The built-in mechanism searches in the AppSettings
   ''' for a given ProviderDllPath, loads this dll and searches for a class inherited from/
   ''' implementing the TServiceImpl with a constructor taking an uri parameter. To avoid
   ''' reflection set the CustomInstanceFactory property.
   ''' </remarks>
   Public Class ServiceImplFactory(Of TServiceImpl)

      Public Sub New(baseUri As Uri)
         _cmisServiceImpl = CreateInstance(baseUri)
      End Sub

#Region "Helper classes"
      Public Delegate Function InstanceFactory(baseUri As Uri) As TServiceImpl
#End Region

#Region "TServiceImpl-Factory"
      Private Shared _customInstanceFactory As InstanceFactory
      ''' <summary>
      ''' Provides injection of 
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Property CustomInstanceFactory As InstanceFactory
         Get
            Return _customInstanceFactory
         End Get
         Set(value As InstanceFactory)
            _customInstanceFactory = value
         End Set
      End Property

      ''' <summary>
      ''' Creates a new instance that implements the ICmisServicesImpl interface
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Shared Function CreateInstance(baseUri As Uri) As TServiceImpl
         If _customInstanceFactory Is Nothing Then
            Dim dllPath As String = System.Configuration.ConfigurationManager.AppSettings("ProviderDllPath")

            If Not String.IsNullOrEmpty(dllPath) Then
               dllPath = dllPath.Replace("[$BaseDirectory]", System.AppDomain.CurrentDomain.BaseDirectory)

               If Not System.IO.File.Exists(dllPath) Then
                  Throw New System.IO.FileNotFoundException(Nothing, dllPath)
               Else
                  Dim assembly As System.Reflection.Assembly = System.Reflection.Assembly.LoadFrom(dllPath)
                  Dim cis As sr.ConstructorInfo() = (From type As Type In assembly.GetTypes()
                                                     Let ci As sr.ConstructorInfo = If(Not type.IsAbstract AndAlso (type.IsPublic OrElse type.IsNestedPublic) AndAlso
                                                                                      GetType(TServiceImpl).IsAssignableFrom(type),
                                                                                      type.GetConstructor(New Type() {GetType(System.Uri)}), Nothing)
                                                     Where ci IsNot Nothing
                                                     Select ci).ToArray()
                  If cis.Length = 0 Then
                     Throw New sr.InvalidFilterCriteriaException(String.Format(My.Resources.ServicesImplNotFound,
                                                                               If(GetType(TServiceImpl).IsInterface, "implement", "inherit"),
                                                                               GetType(TServiceImpl).FullName))
                  Else
                     'generate IL code for _customInstanceFactory
                     Dim method As New sre.DynamicMethod("", GetType(TServiceImpl), New Type() {GetType(System.Uri)})
                     Dim il As sre.ILGenerator = method.GetILGenerator(256)

                     il.Emit(sre.OpCodes.Ldarg_0)
                     il.Emit(sre.OpCodes.Newobj, cis(0))
                     il.Emit(sre.OpCodes.Ret)
                     _customInstanceFactory = CType(method.CreateDelegate(GetType(InstanceFactory)), InstanceFactory)
                  End If
               End If
            End If
         End If

         Return _customInstanceFactory.Invoke(baseUri)
      End Function
#End Region

      Private ReadOnly _cmisServiceImpl As TServiceImpl
      ''' <summary>
      ''' Returns a valid instance
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property CmisServiceImpl As TServiceImpl
         Get
            Return _cmisServiceImpl
         End Get
      End Property

   End Class
End Namespace