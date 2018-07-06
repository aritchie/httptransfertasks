using System;
using System.Windows.Input;
using Plugin.HttpTransferTasks;
using Xamarin.Forms;


namespace Sample
{
    public class NewTaskViewModel : ViewModel
    {
        public NewTaskViewModel()
        {
            this.url = "http://ipv4.download.thinkbroadband.com/1GB.zip";

            this.Save = new Command(async () =>
            {
	            this.ErrorMessage = String.Empty;
	            try
	            {
	                if (!Uri.TryCreate(this.Url, UriKind.Absolute, out _))
	                {
	                    this.ErrorMessage = "Invalid URL";
	                }
	                else if (this.IsUpload && String.IsNullOrWhiteSpace(this.LocalFilePath))
	                {
	                    this.LocalFilePath = "You must enter the file to upload";
	                }

	                if (this.IsUpload)
	                {
	                    CrossHttpTransfers.Current.Upload(this.Url, this.LocalFilePath);
	                }
	                else
	                {
	                    CrossHttpTransfers.Current.Download(this.Url);
	                }
	                if (String.IsNullOrWhiteSpace(this.ErrorMessage))
	                    await App.Current.MainPage.Navigation.PopAsync(true);
	            }
	            catch (Exception ex)
	            {
	                this.ErrorMessage = ex.ToString();
                }
            });
        }


        string errorMsg;
        public string ErrorMessage
        {
            get => this.errorMsg;
            set => this.SetProperty(ref this.errorMsg, value);
        }


        string url;
        public string Url
        {
            get => this.url;
            set => this.SetProperty(ref this.url, value);
        }


        string localFilePath;
        public string LocalFilePath
        {
            get => this.localFilePath;
            set => this.SetProperty(ref this.localFilePath, value);
        }


        bool upload;
        public bool IsUpload
        {
            get => this.upload;
            set => this.SetProperty(ref this.upload, value);
        }


        public ICommand Save { get; }
    }
}
