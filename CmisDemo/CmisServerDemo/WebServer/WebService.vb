
Public Class WebService
   Implements IWebService

   Shared _folder As String = Configuration.ConfigurationManager.AppSettings("folder")

   Public Function ShowObject(objectId As String) As IO.Stream Implements IWebService.ShowObject
      If String.IsNullOrWhiteSpace(objectId) OrElse "{0}".Equals(objectId) Then objectId = "root"

      Dim pos As Integer = objectId.LastIndexOf(";"c)
      Dim versionSeriesId As String = If(pos < 0, objectId, objectId.Substring(0, pos))

      Dim details As String = String.Empty
      If IO.File.Exists(IO.Path.Combine(_folder, versionSeriesId, "metadata")) Then
         'Document

         Dim versions As New List(Of String)
         If IO.Directory.Exists(IO.Path.Combine(_folder, versionSeriesId, "Versionen")) Then
            versions.AddRange(From path As String In IO.Directory.EnumerateFiles(IO.Path.Combine(_folder, versionSeriesId, "Versionen"))
                              Let name As String = path.Split("\").Last
                              Let id As String = versionSeriesId & ";" & name
                              Order By Double.Parse(name)
                              Select name & " <small> <a href=""obj?id=" & id & """>" & id & "</a></small>")
         End If
         If IO.File.Exists(IO.Path.Combine(_folder, versionSeriesId, "pwc")) Then
            versions.Add("pwc" & " <small> <a href=""obj?id=" & versionSeriesId & ";pwc"">" & versionSeriesId & ";pwc</a></small>")
         End If
         versions.Reverse()
         details = "Inhalt<ul><li><a href="" file?id=" & objectId & """>Content</a><li><a href="" meta?id=" & objectId & """>Metadaten</a></ul>" _
            & "Versionen<ul><li>" & String.Join("<li>", versions) & "</ul>" _
            & "Serie<ul><li><a href=""obj?id=" & versionSeriesId & """>" & versionSeriesId & "</a>"
      Else
         'Folder

         If "root".Equals(versionSeriesId) Then versionSeriesId = String.Empty

         Dim subfolders = From folder As String In IO.Directory.EnumerateDirectories(IO.Path.Combine(_folder, versionSeriesId))
                          Where Not IO.File.Exists(IO.Path.Combine(folder, "metadata"))
                          Let id As String = folder.Replace(_folder & "\", String.Empty)
                          Select "<a href=""obj?id=" & id & """>" & IO.Path.GetFileName(folder) & "</a>"
         Dim files = From folder As String In IO.Directory.EnumerateDirectories(IO.Path.Combine(_folder, versionSeriesId))
                     Where IO.File.Exists(IO.Path.Combine(folder, "metadata"))
                     Let id As String = folder.Replace(_folder & "\", String.Empty)
                     Select "<a href=""obj?id=" & id & """>" & IO.Path.GetFileName(folder) & "</a>"

         details = "Verzeichnisse<ul>"
         If subfolders.Count > 0 Then
            details &= "<li>" & String.Join("<li>", subfolders)
         Else
            details &= "<i>keine Unterverzeichnisse</i>"
         End If
         details &= "</ul>Dokumente<ul>"
         If files.Count > 0 Then
            details &= "<li>" & String.Join("<li>", files)
         Else
            details &= "<i>keine Dokumente</i>"
         End If
         details &= "</ul>"
      End If

      Return New IO.MemoryStream(Text.Encoding.UTF8.GetBytes("<!DOCTYPE html>" _
         & "<html><head><meta http-equiv=""Content-Type"" content=""text/html; charset=iso-8859-1"" />" _
         & "<title> CmisDemo - WebService</title>" _
         & "</head>" _
         & "<body>" _
         & "<h1>" & objectId & "</h1>" _
         & "<p>" & details & "</p>" _
         & "</body></html>"))

   End Function

   Public Function GetContent(objectId As String) As System.IO.Stream Implements IWebService.GetContent

      Dim pos As Integer = objectId.LastIndexOf(";"c)
      Dim versionSeriesId As String
      Dim filename As String
      If pos < 0 Then
         versionSeriesId = objectId

         Dim dir As String = IO.Path.Combine(_folder, versionSeriesId, "Versionen")
         Dim maximum = (From path As String In IO.Directory.EnumerateFiles(dir)
                        Let name As String = path.Split("\").Last
                        Select Double.Parse(name.Replace(".", ","))).Max()
         filename = (From path As String In IO.Directory.EnumerateFiles(dir)
                     Let name As String = path.Split("\").Last
                     Where Double.Parse(name.Replace(".", ",")) = maximum
                     Select path).Single()
      Else
         versionSeriesId = objectId.Substring(0, pos)
         Dim version As String = objectId.Substring(pos + 1)
         If Not version.Equals("pwc") Then
            filename = IO.Path.Combine(_folder, versionSeriesId, "Versionen", version)
         Else
            filename = IO.Path.Combine(_folder, versionSeriesId, version)
         End If
      End If

      Dim metaXml As String = IO.File.ReadAllText(IO.Path.Combine(_folder, versionSeriesId, "metadata"))
      Dim mimeTypeStart As Integer = metaXml.IndexOf("<MimeType>") + "<MimeType>".Length
      Dim mimeTypeEnd As Integer = metaXml.IndexOf("</MimeType>")
      Dim mimeType As String = metaXml.Substring(mimeTypeStart, mimeTypeEnd - mimeTypeStart)

      ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.ContentType = mimeType
      ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.Headers.Add("content-disposition", "attachment; filename=""" _
                                                                                   & IO.Path.GetFileName(IO.Path.Combine(_folder, versionSeriesId)) & """")
      Return IO.File.OpenRead(filename)
   End Function

   Private Function GetMetadata(objectId As String) As IO.Stream Implements IWebService.GetMetadata
      ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml"

      Dim pos As Integer = objectId.LastIndexOf(";"c)
      Dim versionSeriesId As String = If(pos < 0, objectId, objectId.Substring(0, pos))
      Dim filename As String = IO.Path.Combine(_folder, versionSeriesId, "metadata")
      Dim xml As String = IO.File.ReadAllText(filename)
      xml = xml.Replace(" encoding=""utf-16""", String.Empty)
      Return New IO.MemoryStream(Text.Encoding.UTF8.GetBytes(xml))
   End Function
End Class
