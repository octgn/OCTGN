namespace Octgn.Data.Database.Entities
{
    using System;
    using System.Collections.Generic;

    public class Game
    {
        public virtual int Real_Id { get; set; }
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Filename { get; set; }
        public virtual Version Version { get; set; }
        public virtual int Card_Width { get; set; }
        public virtual int Card_Height { get; set; }
        public virtual string Card_Back { get; set; }
        public virtual string Deck_Sections { get; set; }
        public virtual string Shared_Deck_Sections { get; set; }
        public virtual string File_Hash { get; set; }
        public virtual IList<Set> Sets { get; set; } 
    }
}
