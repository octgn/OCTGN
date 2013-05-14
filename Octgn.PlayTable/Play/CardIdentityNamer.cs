namespace Octgn.Play
{
    using Octgn.Play.Gui;

    internal class CardIdentityNamer
    {
        public CardRun Target { get; set; }

        public void Rename(object sender, RevealEventArgs e)
        {
            var id = (CardIdentity) sender;
            id.Revealed -= this.Rename;
            CardIdentity newId = e.NewIdentity;
            if (newId.Model != null)
                this.Target.SetCardModel(newId.Model);
            else
                newId.Revealed += this.Rename;
        }
    }
}