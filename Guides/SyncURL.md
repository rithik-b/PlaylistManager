# Creating a Syncable Playlist
Run a website that hosts playlists that update content (e.g Map Pools)?  This guide should help you set up a syncable playlist in Beat Saber.
1) Open the json file for the playlist in a text editor. If you use a script to generate playlists instead, open the script.
2) Add the following key ``syncURL`` (spelled in the exact same way). For the value use the download URL for the playlist. Make sure you have a **static URL** setup for your website for hosting the playlist, i.e. a URL that **never changes** and always has the **latest version** of the playlist you're hosting. Here is an example of what the line would look like
```"syncURL": "http://somewebsite.com/playlist.bplist",```
3) That's it, now people should be able to automatically sync their playlists with the latest version available on your website!

