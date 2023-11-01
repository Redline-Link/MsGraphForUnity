using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using UnityEngine;
using UnityEngine.UI;

namespace MicrosoftGraphForUnity.Examples
{
    /// <summary>
    /// This example shows how to query OneDrive from a Microsoft Graph account and present the results in a list.
    /// </summary>
    public class OneDriveManager : MonoBehaviour
    {
        [Header("Misc")]
        [SerializeField]
        private MicrosoftGraphManager graphManager;
        [SerializeField]
        private UIDriveItemElement driveItemPrefab;
        [SerializeField]
        private Sprite placeHolderThumbnail;
        [SerializeField]
        private Sprite fileThumbnail;
        [SerializeField]
        private Sprite folderThumbnail;
        [SerializeField]
        private Sprite imageThumbnail;

        [Header("UI")]
        [SerializeField]
        private InputField inputField;
        [SerializeField]
        private Button searchButton;
        [SerializeField]
        private Text searchButtonText;
        [SerializeField]
        private Transform contentRoot;

        private List<UIDriveItemElement> foundItems;
        private bool isSearching;
        private bool cancelSearch;

        private void Start()
        {
            searchButton.onClick.AddListener(HandleOnSearchButtonClick);
        }

        private async void SignOut()
        {
            await graphManager.AuthenticationService.SignOutAsync();
        }

        private async void HandleOnSearchButtonClick()
        {
            if (isSearching)
            {
                cancelSearch = true;
                return;
            }
            
            if (string.IsNullOrWhiteSpace(inputField.text))
            {
                return;
            }

            isSearching = true;
            searchButtonText.text = "Cancel";
            
            var myDriveData = await graphManager.Client.Me.Drive.GetAsync();
            if (myDriveData == null)
            {
                Debug.LogError("can not get your drive, stopping");
                return;
            }
            string myDriveId = myDriveData.Id;
            // v2
            // var searchResult = await SearchDrive(graphManager.Client.Me.Drive, inputField.text);
            // v5
            var searchResult = await SearchDrive(graphManager.Client.Drives[myDriveId], inputField.text);
            // var searchResult = await graphManager.Client.Drive.Items["FCD4E627B66473E3!3519"].Children.Request().GetAsync();

            if (searchResult == null || !searchResult.Any())
            {
                isSearching = false;
                cancelSearch = false;
                searchButtonText.text = "Search";
                return;
            }

            if (foundItems != null && foundItems.Any())
            {
                foreach (var searchItem in foundItems)
                {
                    Destroy(searchItem.gameObject);
                }
            }
            
            foundItems = new List<UIDriveItemElement>();
            var counter = 0;
            foreach (var driveItem in searchResult)
            {
                if (cancelSearch)
                {
                    break;
                }
                var item = Instantiate(driveItemPrefab, contentRoot);
                item.transform.SetAsLastSibling();
                item.text.text = driveItem.Name;
                Debug.Log("Downloading " + driveItem.Name);
                // v2 way to get the file
                // using (var data = await graphManager.Client.Me.Drive.Items[driveItem.Id].Content.Request().GetAsync())
                // {
                // counter++;
                // Debug.Log("Downloaded data " + counter);
                // }
                //
                // // v5 can not get drive item directly
                // await using var data = await graphManager.Client.Drives[myDriveId].Items[driveItem.Id].Content.GetAsync();
                // counter++;
                // Debug.Log("Downloaded data " + counter);

                // disable image preview
                //var sprite = await DownloadDriveItemThumbnail(graphManager.Client.Drives[myDriveId], driveItem.Id);
                // item.image.sprite = sprite != null ? sprite : placeHolderThumbnail;
                if (driveItem.Folder != null || driveItem.SpecialFolder != null)
                    item.image.sprite = folderThumbnail;
                else if (driveItem.Image != null)
                    item.image.sprite = imageThumbnail;
                else
                    item.image.sprite = fileThumbnail;
                
                foundItems.Add(item);
            }
            
            isSearching = false;
            cancelSearch = false;
            searchButtonText.text = "Search";
        }

        /// <summary>
        /// Search the drive by the given <see href="https://docs.microsoft.com/en-us/graph/query-parameters">OData Query</see> string.
        /// </summary>
        /// <param name="drive">Target drive to search at.</param>
        /// <param name="query">Search text query</param>
        /// <returns>Found DriveItems</returns>
        // private async Task<List<DriveItem>> SearchDrive(IDriveRequestBuilder drive, string query)
        private async Task<List<DriveItem>> SearchDrive(Microsoft.Graph.Drives.Item.DriveItemRequestBuilder drive, string query)
        {
            // v2
            // var search = await drive.Search(query).Request().GetAsync();
            // v5
            var search = await drive.SearchWithQ(query).GetAsSearchWithQGetResponseAsync();
            return search?.Value?.ToList();
        }

        /// <summary>
        /// Download the primary mid sized thumbnail as a Sprite.
        /// </summary>
        /// <param name="drive">Target drive.</param>
        /// <param name="itemId">Source item id.</param>
        /// <returns>Thumbnail as Sprite or null if the item has no thumbnail.</returns>
        private async Task<Sprite> DownloadDriveItemThumbnail(Microsoft.Graph.Drives.Item.DriveItemRequestBuilder drive, string itemId)
        {
            // TODO too many random exception here, need research
            // no thumbnail for you!
            await Task.Delay(10);
            return null;
            
            // var thumbnails = await drive.Items[itemId].Thumbnails.GetAsync();
            // if (thumbnails == null)
            // {
            //     return null;
            // }
            // if (thumbnails?.Value?.First() == null)
            // {
            //     return null;
            // }
            //
            // ThumbnailSet thumbnail = thumbnails.Value?.First();
            // if (thumbnail == null)
            // {
            //     return null;
            // }
            //
            // // var content = await drive.Items[itemId].Thumbnails[thumbnail.Id]["medium"].Content.Request().GetAsync();
            // var contentReq = await drive.Items[itemId].Thumbnails[thumbnail.Id].GetAsync();
            // if (contentReq == null)
            //     return null;
            //
            // var content = contentReq.Medium;
            // if (content == null || content.Content == null)
            //     return null;
            //
            // MemoryStream ms = new MemoryStream(content.Content);
            //
            // using (var reader = new MemoryStream())
            // {
            //     await ms.CopyToAsync(reader);
            //     var data  = reader.ToArray();
            //     var texture = new Texture2D(0, 0);
            //     texture.LoadImage(data);
            //     return Sprite.Create(
            //         texture, 
            //         new Rect(0, 0, texture.width, texture.height), 
            //         new Vector2(0.5f, 0.5f));
            // }
        }
    }
}
