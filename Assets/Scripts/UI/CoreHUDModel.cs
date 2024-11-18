using Core.Controller;
using Modules.ServiceLocator;
using Modules.UIService;
using Services.GamePlayerDataService;

namespace UI
{
    public class CoreHUDModel : UIModel
    {
        public int Score => Controller.Score;
        public int TopScore => Data.PlayerData.MaxScore;

        private BoardController Controller { get; set; }
        private GamePlayerDataService Data { get; set; }

        [Inject]
        private void Inject(BoardController controller, GamePlayerDataService playerDataService)
        {
            Controller = controller;
            Data = playerDataService;
        }

        public CoreHUDModel()
        {
            Container.Inject(this);
        }
    }
}