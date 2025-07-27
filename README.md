ChessUIManager + public void TriggerPawnPromotion
[SerializeField] private PiecesCreator piecesCreator;
Add PromotionUI.cs
Chess game controller + public void HandlePawnPromotion


I’m using a GameMaster object, which contains the scripts for the PiecesCreator and the ChessGameController.I assigned it to the UI.
In the UI, I created a RestartButton. I removed its original Button component and replaced it with my custom UIButton, 
UIInputHandler, and UIInputReceiver, connecting them to the ChessGameController through the GameMaster.
Now, I need to implement functionality for the Exit Button.
I also need to figure out what to do with PromotionUI or UIManager to complete the pawn promotion feature.
Currently, en passant is missing.
Once these features are completed, I plan to upgrade the game to support online multiplayer.

