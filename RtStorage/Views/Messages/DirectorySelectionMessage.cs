using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Livet.Messaging;
using Livet.Messaging.IO;

namespace RtStorage.Views.Messages
{
    public class DirectorySelectionMessage : ResponsiveInteractionMessage<string>
    {
        public DirectorySelectionMessage()
        {
        }

        public DirectorySelectionMessage(string messageKey)
            : base(messageKey)
        {
        }

        protected override Freezable CreateInstanceCore()
        {
            return new DirectorySelectionMessage();
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(DirectorySelectionMessage), new UIPropertyMetadata(null));

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register("SelectedPath", typeof(string), typeof(DirectorySelectionMessage), new UIPropertyMetadata(null));

    }
}
