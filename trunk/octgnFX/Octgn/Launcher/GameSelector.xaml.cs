using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Octgn.Definitions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;

namespace Octgn.Launcher
{
   public sealed partial class GameSelector : UserControl
   {
      private IList<Data.Game> games;
      private int selectedIdx = 0;
      private static readonly Duration dt = new Duration(TimeSpan.FromMilliseconds(500));

      public GameSelector()
      {
         InitializeComponent();
         if (DesignerProperties.GetIsInDesignMode(this)) return;
         games = Program.GamesRepository.Games;
         Create3DItems();
      }

      public Game Game
      {
         get
         {
            if (games.Count == 0) return null;
            //var serializer = new BinaryFormatter();
            //var memStream = new MemoryStream(games[selectedIdx].Data);
            //GameDef def = (GameDef)serializer.Deserialize(memStream);
            //return new Game(def);
            return new Game(GameDef.FromO8G(games[selectedIdx].Filename));
         }
      }

      private void Create3DItems()
      {
         container.Children.Clear();

         var rnd = new Random();
         var i = 0;

         foreach (var game in games)
         {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = game.GetCardBackUri();
            bmp.EndInit();

            var halfWidth = game.CardWidth * 1.5 / game.CardHeight;
            var item = new ModelUIElement3D()
            {
               Model = new GeometryModel3D()
               {
                  Geometry = new MeshGeometry3D()
                  {
                     Positions = new Point3DCollection()
                                {
                                    new Point3D(-halfWidth,0,-1),
                                    new Point3D(-halfWidth,3,-1),
                                    new Point3D(halfWidth,3,-1),
                                    new Point3D(halfWidth,0,-1)
                                },
                     TriangleIndices = new Int32Collection() { 0, 2, 1, 3, 2, 0 },
                     TextureCoordinates = new PointCollection() { new Point(0, 1), new Point(0, 0), new Point(1, 0), new Point(1, 1) }
                  },
                  Material = new DiffuseMaterial(new ImageBrush(bmp)),
                  Transform = new Transform3DGroup()
                  {
                     Children = new Transform3DCollection()
                                {
                                    new RotateTransform3D() 
                                    {
                                        Rotation = new AxisAngleRotation3D(new Vector3D(0,1,0), i == 0 ? 0 : -40)
                                    },
                                    new TranslateTransform3D(i == 0 ? 0 : 0.5+i, 0, i != 0 ? -1 : 0)
                                }
                  }
               }
            };
            item.MouseDown += Select;
            container.Children.Add(item);
            ++i;
         }

         if (games.Count > 0)
         {
            selectedIdx = 0;
            nameText.Text = games[0].Name;
         }
      }

      private void Select(object sender, RoutedEventArgs e)
      {
         e.Handled = true;
         var idx = container.Children.IndexOf((Visual3D)sender);
         if (idx == selectedIdx) return;
         for (int i = 0; i < container.Children.Count; ++i)
         {
            var item = (ModelUIElement3D)container.Children[i];
            var rotate = (RotateTransform3D)((Transform3DGroup)item.Model.Transform).Children[0];
            var translate = (TranslateTransform3D)((Transform3DGroup)item.Model.Transform).Children[1];

            var anim = new DoubleAnimation(i == idx ? 0 : -1, dt, FillBehavior.HoldEnd) { DecelerationRatio = 0.65 };
            translate.BeginAnimation(TranslateTransform3D.OffsetZProperty, anim, HandoffBehavior.SnapshotAndReplace);
            anim = new DoubleAnimation(i - idx + 0.5 * Math.Sign(i - idx), dt, FillBehavior.HoldEnd) { DecelerationRatio = 0.65 };
            translate.BeginAnimation(TranslateTransform3D.OffsetXProperty, anim, HandoffBehavior.SnapshotAndReplace);
            anim = new DoubleAnimation(i < idx ? 40 : i > idx ? -40 : 0, dt, FillBehavior.HoldEnd) { DecelerationRatio = 0.65 };
            ((AxisAngleRotation3D)rotate.Rotation).BeginAnimation(AxisAngleRotation3D.AngleProperty, anim, HandoffBehavior.SnapshotAndReplace);
         }
         selectedIdx = idx;
         nameText.Text = games[idx].Name;
      }
   }
}