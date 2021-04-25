using System;
using goFriend.DataModel;
using Xamarin.Forms.GoogleMaps;

namespace goFriend.Controls
{
    public class DphPin
    {
        public DphPin()
        {
            Pin = new Pin();
        }

        public string IconUrl { get; set; }
        public string Url { get; set; }
        public UserType UserRight { get; set; }
        public Friend User { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }

        public bool IsDraggable {
            get => Pin.IsDraggable;
            set => Pin.IsDraggable = value;
        }

        public Position Position {
            get => Pin.Position;
            set => Pin.Position = value;
        }

        public PinType Type {
            get => Pin.Type;
            set => Pin.Type = value;
        }

        public Pin Pin { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Pin.Label = _title;
            }
        }

        private string _subTitle1;
        public string SubTitle1
        {
            get => _subTitle1;
            set
            {
                _subTitle1 = value;
                Pin.Address = $"{_subTitle1}{Environment.NewLine}{_subTitle2}";
            }
        }

        private string _subTitle2;
        public string SubTitle2
        {
            get => _subTitle2;
            set
            {
                _subTitle2 = value;
                Pin.Address = $"{_subTitle1}{Environment.NewLine}{_subTitle2}";
            }
        }
    }
}
