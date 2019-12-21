using MapKit;

namespace goFriend.iOS
{
    public class CustomAnnotationView : MKAnnotationView
    {
        public CustomAnnotationView(IMKAnnotation annotation, string id)
            : base(annotation, id) {}
    }
}