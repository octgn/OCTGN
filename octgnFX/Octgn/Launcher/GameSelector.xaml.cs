using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Octgn.Data;
using Octgn.Definitions;

namespace Octgn.Launcher
{
    public sealed partial class GameSelector
    {
        private static readonly Duration Dt = new Duration(TimeSpan.FromMilliseconds(500));
        private readonly IList<Data.Game> _games;
        private int _selectedIdx;

        public GameSelector()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            _games = Program.GamesRepository.Games;
            Create3DItems();
        }

        public Game Game
        {
            get
            {
                return _games.Count == 0 ? null : new Game(GameDef.FromO8G(Path.Combine(GamesRepository.BasePath,"Defs",_games[_selectedIdx].Filename)));
                //var serializer = new BinaryFormatter();
                //var memStream = new MemoryStream(games[selectedIdx].Data);
                //GameDef def = (GameDef)serializer.Deserialize(memStream);
                //return new Game(def);
            }
        }

        private void Create3DItems()
        {
            container.Children.Clear();

            //var rnd = new Random();
            int i = 0;

            foreach (Data.Game game in _games)
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = game.GetCardBackUri();
                bmp.EndInit();

                double halfWidth = game.CardWidth*1.5/game.CardHeight;
                var item = new ModelUIElement3D
                               {
                                   Model = new GeometryModel3D
                                               {
                                                   Geometry = new MeshGeometry3D
                                                                  {
                                                                      Positions = new Point3DCollection
                                                                                      {
                                                                                          new Point3D(-halfWidth, 0, -1),
                                                                                          new Point3D(-halfWidth, 3, -1),
                                                                                          new Point3D(halfWidth, 3, -1),
                                                                                          new Point3D(halfWidth, 0, -1)
                                                                                      },
                                                                      TriangleIndices =
                                                                          new Int32Collection {0, 2, 1, 3, 2, 0},
                                                                      TextureCoordinates =
                                                                          new PointCollection
                                                                              {
                                                                                  new Point(0, 1),
                                                                                  new Point(0, 0),
                                                                                  new Point(1, 0),
                                                                                  new Point(1, 1)
                                                                              }
                                                                  },
                                                   Material = new DiffuseMaterial(new ImageBrush(bmp)),
                                                   Transform = new Transform3DGroup
                                                                   {
                                                                       Children = new Transform3DCollection
                                                                                      {
                                                                                          new RotateTransform3D
                                                                                              {
                                                                                                  Rotation =
                                                                                                      new AxisAngleRotation3D
                                                                                                      (new Vector3D(0, 1,
                                                                                                                    0),
                                                                                                       i == 0 ? 0 : -40)
                                                                                              },
                                                                                          new TranslateTransform3D(
                                                                                              i == 0 ? 0 : 0.5 + i, 0,
                                                                                              i != 0 ? -1 : 0)
                                                                                      }
                                                                   }
                                               }
                               };
                item.MouseDown += Select;
                container.Children.Add(item);
                ++i;
            }

            if (_games.Count <= 0) return;
            _selectedIdx = 0;
            nameText.Text = _games[0].Name;
        }

        private void Select(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            int idx = container.Children.IndexOf((Visual3D) sender);
            if (idx == _selectedIdx) return;
            for (int i = 0; i < container.Children.Count; ++i)
            {
                var item = (ModelUIElement3D) container.Children[i];
                var rotate = (RotateTransform3D) ((Transform3DGroup) item.Model.Transform).Children[0];
                var translate = (TranslateTransform3D) ((Transform3DGroup) item.Model.Transform).Children[1];

                var anim = new DoubleAnimation(i == idx ? 0 : -1, Dt, FillBehavior.HoldEnd) {DecelerationRatio = 0.65};
                translate.BeginAnimation(TranslateTransform3D.OffsetZProperty, anim, HandoffBehavior.SnapshotAndReplace);
                anim = new DoubleAnimation(i - idx + 0.5*Math.Sign(i - idx), Dt, FillBehavior.HoldEnd)
                           {DecelerationRatio = 0.65};
                translate.BeginAnimation(TranslateTransform3D.OffsetXProperty, anim, HandoffBehavior.SnapshotAndReplace);
                anim = new DoubleAnimation(i < idx ? 40 : i > idx ? -40 : 0, Dt, FillBehavior.HoldEnd)
                           {DecelerationRatio = 0.65};
                (rotate.Rotation).BeginAnimation(AxisAngleRotation3D.AngleProperty, anim,
                                                 HandoffBehavior.SnapshotAndReplace);
            }
            _selectedIdx = idx;
            nameText.Text = _games[idx].Name;
        }
    }
}