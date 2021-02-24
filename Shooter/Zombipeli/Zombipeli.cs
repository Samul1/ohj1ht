using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
/// @author Samuli Juutinen
/// @version 23.02.2021
/// <summary>
/// Ylhäältäpäin kuvattu zombie räiskintä peli.
/// </summary>
/// 

public class ZombiPeli : PhysicsGame
{
    #region Atribuutit

    // Luodaan uudet Vectorit pelaajan liikuttamista varten.
    private Vector nopeusYlos = new Vector(0, 200);
    private Vector nopeusAlas = new Vector(0, -200);
    private Vector nopeusVasen = new Vector(-200, 0);
    private Vector nopeusOikea = new Vector(200, 0);
    
    // Kentän rajat
    private PhysicsObject vasenReuna;
    private PhysicsObject oikeaReuna;
    private PhysicsObject ylaReuna;
    private PhysicsObject alaReuna;
    
    // Luodaan pelaaja attribuutti luokan sisällä
    private PhysicsObject pelaaja;
    // Luodaan aseen atribuutti.
    private AssaultRifle pelaajanAse;
    // Tarkastetaan onko pelaaja hengissä
    private bool pelaajaKuoli = false;
    
    private bool pelaajaKatsooVasemmalle = false;
     
    // Laskurit
    private IntMeter pelaajanPisteet;
    private IntMeter zombiLaskuri;
    
     // HighScore ikkuna
    private ScoreList topLista = new ScoreList(10, false, 0);
   
    // Rakennuksen palikoiden koko
    private const double RUUDUN_LEVEYS = 25;
    private const double RUUDUN_KORKEUS = 25;
   
    #endregion
    
    /// <summary>
    /// Pelin suoritus alkaa tästä.
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(618, 720);
        Level.Background.Image = LoadImage("ZombiKuva");
        
        topLista = DataStorage.TryLoad<ScoreList>(topLista, "pisteet.xml");
        AloitusValikko();
    }

    #region Valikko

    /// <summary>
    /// Pelin aloitusvalikko.
    /// </summary>
    public void AloitusValikko()
    { 
        MultiSelectWindow alkuValikko = new MultiSelectWindow("Zombie Survival", "Aloita peli", "Parhaat pisteet", "Lopeta");
        alkuValikko.AddItemHandler(0, AloitaPeli);
        alkuValikko.AddItemHandler(1, ParhaatPisteet);
        alkuValikko.AddItemHandler(2, Exit);
        alkuValikko.DefaultCancel = 2;
        alkuValikko.Color = Color.Green;
        Mouse.IsCursorVisible = true;
        Add(alkuValikko);
    }

    /// <summary>
    /// Aloittaa pelin.
    /// </summary>
    public void AloitaPeli()
    {
        LuoKentta();
        AsetaOhjaimet();
        LisaaLaskurit();
        ZombiAjastin();
    }

    /// <summary>
    /// Parhaat pisteet ikkuna.
    /// Tuo Parhaat pisteet tiedostosta tallennetut pisteet.
    /// </summary>
    public void ParhaatPisteet()
    {
        HighScoreWindow topIkkuna = new HighScoreWindow ("Parhaat pisteet", topLista);
        topIkkuna.Closed += TallennaPisteet;
        Add(topIkkuna);
    }

    /// <summary>
    /// Aliohjelma tallentaa parhaat pisteet
    /// pisteet.xml tiedostoon.
    /// </summary>
    /// <param name="lahettaja">lahettaja</param>
    public void TallennaPisteet(Window lahettaja)
    {
        DataStorage.Save<ScoreList>(topLista, "pisteet.xml");
        AloitusValikko();
    }

    #endregion

    #region Laskuri ja ajastin

    /// <summary>
    /// Laittaa laskurit näytölle.
    /// </summary>
    public void LisaaLaskurit()
    {
        pelaajanPisteet = LuoPisteLaskuri(Screen.Left + 100.0, Screen.Top - 100.0);
        zombiLaskuri = LuoZombiLaskuri(Screen.Right - 100.0, Screen.Top - 100.0);
        Label zombiLaskuriTeksti = new Label("Zombeja jäljellä:");
        zombiLaskuriTeksti.X = Screen.Right - 220;
        zombiLaskuriTeksti.Y = Screen.Top - 100;
        zombiLaskuriTeksti.Color = Color.Black;
        zombiLaskuriTeksti.TextColor = Color.Green;
        zombiLaskuriTeksti.BorderColor = Color.Black;
        Add(zombiLaskuriTeksti);
    }

    /// <summary>
    /// Pelaajan pistelaskuri.
    /// </summary>
    /// <param name="x">X-koordinaatti.</param>
    /// <param name="y">Y-koordinaatti.</param>
    /// <returns>laskuri</returns>
    IntMeter LuoPisteLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);

        Label naytto = new Label();
        naytto.BindTo(laskuri);
        naytto.X = x;
        naytto.Y = y;
        naytto.TextColor = Color.BloodRed;
        naytto.BorderColor = Color.Black;
        naytto.Color = Color.Black;
        Add(naytto);

        return laskuri;
    }

    /// <summary>
    /// Zombi laskuri.
    /// Näyttää montako zombia on maailmassa.
    /// </summary>
    /// <param name="x">x-koordinaatti.</param>
    /// <param name="y">Y-koordinaatti.</param>
    /// <returns>laskuri</returns>
    IntMeter LuoZombiLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);

        Label naytto = new Label();
        naytto.BindTo(laskuri);
        naytto.X = x;
        naytto.Y = y;
        naytto.TextColor = Color.Green;
        naytto.BorderColor = Color.Black;
        naytto.Color = Color.Black;
        Add(naytto);

        return laskuri;
    }

    /// <summary>
    /// Ajastaa zombien ilmestymiset kentän rajalle.
    /// </summary>
    public void ZombiAjastin()
    {
        // 60 sekunnin ajan luodaan zombi kahden sekunnin välein.
        Timer ajastin = new Timer();
        ajastin.Interval = 2.0;
        ajastin.Timeout += LuoZombi;
        ajastin.Start(30);
    }

    #endregion

    #region Pelikenttan ja pelaajan luonti.

    /// <summary>
    /// Kutsutaan pelaaja sekä eka zombi.
    /// Luodaan pelikenttä.
    /// Rajat ja kamera asetukset.
    /// </summary>
    public void LuoKentta()
    {
        LuoRakennus();

        pelaaja = LuoPelaaja(-200.0, 0.0);
        // Peelikentän ja ikkunan koko
        Level.Size = new Vector(1920, 1080);
        SetWindowSize(1920, 1080);
        Level.Background.Image = LoadImage("maasto");
        Camera.Zoom(3.0);
        Camera.Follow(pelaaja);

        // Asetetaan kentän rajat
        vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Tag = "reuna";
        vasenReuna.Restitution = 1.0;
        vasenReuna.IsVisible = false;

        oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Tag = "reuna";
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.IsVisible = false;

        alaReuna = Level.CreateBottomBorder();
        alaReuna.Tag = "reuna";
        alaReuna.Restitution = 1.0;
        alaReuna.IsVisible = false;

        ylaReuna = Level.CreateTopBorder();
        ylaReuna.Tag = "reuna";
        ylaReuna.Restitution = 1.0;
        ylaReuna.IsVisible = false;

        
    }
    #region Rakennus
    /// <summary>
    /// Luo pelin rakennuksen.
    /// </summary>
    public void LuoRakennus()
    {

        TileMap kentta = TileMap.FromLevelAsset("ShooterKentta");
        kentta.SetTileMethod('x', LuoSeinat, "Seina");
        kentta.SetTileMethod('v', LuoIkkunat);
        kentta.SetTileMethod('l', LuoLattia, "lattia");
        kentta.Execute(RUUDUN_LEVEYS, RUUDUN_KORKEUS);
    }

    /// <summary>
    /// Luodaan seinät.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">palikan leveys</param>
    /// <param name="korkeus">palikan korkeus</param>
    /// <param name="kuvaNimi">palikan kuva</param>
    public void LuoSeinat(Vector paikka, double leveys, double korkeus, string kuvaNimi)
    {
        PhysicsObject seina = new PhysicsObject(leveys, korkeus);
        seina.Position = paikka;
        seina.Image = LoadImage(kuvaNimi);
        seina.Tag = "seina";
        seina.Restitution = 1.0;
        seina.MakeStatic();
        Add(seina);
    }

    /// <summary>
    /// Luodaan ikkunat
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">ikkunan leveys</param>
    /// <param name="korkeus">ikkunan korkaus</param>
    public void LuoIkkunat(Vector paikka, double leveys, double korkeus)
    {
        GameObject ikkuna = new GameObject(leveys, korkeus);
        ikkuna.Position = paikka;
        ikkuna.Color = Color.Aqua;
        ikkuna.Tag = "ikkuna";
        Add(ikkuna);
    }

    /// <summary>
    /// luodaan lattia palikat.
    /// </summary>
    /// <param name="paikka">paikka</param>
    /// <param name="leveys">palikan leveys</param>
    /// <param name="korkeus">palikan korkaus</param>
    public void LuoLattia(Vector paikka, double leveys, double korkeus, string kuvaNimi)
    {
        GameObject lattia = new GameObject(leveys, korkeus);
        lattia.Position = paikka;
        lattia.Image = LoadImage(kuvaNimi);
        lattia.Tag = "lattia";

        Add(lattia, 0);
    }

    #endregion

    #endregion

    #region Pelaaja ja pelaajan ase

    /// <summary>
    /// Luo pelaaja olion
    /// </summary>
    /// <param name="x">Pelaajan x-koordinaatit</param>
    /// <param name="y">Pelaajan y-koordinaatit</param>
    /// <returns></returns>
    PhysicsObject LuoPelaaja(double x, double y)
    {
        PhysicsObject pelaaja = new PhysicsObject(25.0, 25.0);
        pelaaja.Shape = Shape.Circle;
        pelaaja.X = x;
        pelaaja.Y = y;
        pelaaja.Restitution = 0.0;
        pelaaja.CanRotate = false;
        pelaaja.Tag = "pelaaja";
        pelaaja.Image = LoadImage("survivor");
        Add(pelaaja, 1);
        pelaaja.MirrorImage();
        AddCollisionHandler<PhysicsObject, Zombi>(pelaaja, ZombiOsuuPelaajaan);

        pelaajanAse = new AssaultRifle(30, 10);
        //pelaajanAse.Ammo.Value = 100; // Ammusten määrä
        pelaajanAse.InfiniteAmmo = true;
        pelaajanAse.AttackSound = null;
        pelaajanAse.CanHitOwner = false;
        pelaajanAse.X = 5;
        pelaaja.Add(pelaajanAse);

        return pelaaja;
    }

    #endregion

    #region Zombi

    /// <summary>
    /// Luodaan zombit kentälle.
    /// </summary>
    public void LuoZombi()
    {
        List<Vector> polkuAla = new List<Vector>();
        polkuAla.Add(new Vector(-185, Level.Top - 350));
        polkuAla.Add(new Vector(-185, Level.Top - 550));
        LuoZombi(RandomGen.NextDouble(-200, 200), Level.Bottom + 20, polkuAla); // alareuna spawni

        List<Vector> polkuYla = new List<Vector>();
        polkuYla.Add(new Vector(-185, Level.Top - 350));
        polkuYla.Add(new Vector(-185, Level.Top - 550));
        LuoZombi(RandomGen.NextDouble(-200, 200), Level.Top - 20, polkuYla); // yläreuna spawni

        List<Vector> polkuVasen = new List<Vector>();
        polkuVasen.Add(new Vector(Level.Left + 700.0, 20.0));
        LuoZombi(Level.Left + 50, RandomGen.NextDouble(-200, 200), polkuVasen); // vasenreuna spawni

        List<Vector> polkuOikea = new List<Vector>();
        polkuOikea.Add(new Vector(Level.Right - 450, 20.0));
        LuoZombi(Level.Right - 50, RandomGen.NextDouble(-200, 200), polkuOikea); // oikeareuna spawni


    }

    /// <summary>
    /// Luo zombin
    /// </summary>
    /// <param name="x">zombin x-koordinaatit</param>
    /// <param name="y">zombin y-koordinaatit</param>
    /// <returns></returns>
    Zombi LuoZombi(double x, double y, List<Vector> polku)
    {
        Zombi zombi = new Zombi(25.0, 25.0, new Color[] { Color.Green, Color.Red });
        zombi.Shape = Shape.Circle;
        zombi.X = x;
        zombi.Y = y;
        zombi.Restitution = 0.0;
        zombi.Tag = "zombi";
        zombi.Image = LoadImage("zombi");
        zombi.CanRotate = false;
        zombiLaskuri.Value += 1;
        Add(zombi, 1);

        PathFollowerBrain polkuAivo = new PathFollowerBrain(50);
        polkuAivo.Path = polku;
        FollowerBrain seuraajaAivo = new FollowerBrain(pelaaja);
        seuraajaAivo.Speed = 100;
        seuraajaAivo.DistanceFar = 400;
        seuraajaAivo.DistanceClose = 50;
        seuraajaAivo.FarBrain = polkuAivo;
        seuraajaAivo.StopWhenTargetClose = false;
        zombi.Brain = seuraajaAivo;
        return zombi;
    }

    /// <summary>
    /// Hoidellaan zombin osuminen pelaajaan.
    /// </summary>
    /// <param name="pelaaja">PhysicsObject pelaaja</param>
    /// <param name="kohde">Zombi zombi</param>
    public void ZombiOsuuPelaajaan(PhysicsObject pelaaja, Zombi kohde)
    {
        pelaaja.Destroy();
        pelaajaKuoli = true;
        GameOver();
    }

    #endregion

    #region Aseen kayttaytyminen

    /// <summary>
    /// Hoidellaan ammuksen osuminen
    /// </summary>
    /// <param name="ammus">ammus</param>
    /// <param name="kohde">zombi</param>
    public void AmmusOsuiZombiin(PhysicsObject ammus, Zombi kohde)
    {
        if (kohde.Tag.ToString() == "zombi" )
        {
            pelaajanPisteet.Value += 1;
            zombiLaskuri.Value -= 1;
            kohde.OtaVastaanOsuma();
            if(zombiLaskuri.Value == 0)
            {
                GameOver();
            }
        }
        ammus.Destroy();
    }

    /// <summary>
    /// Hoidellaan ammuksen osuminen seinään tai kentän reunoihin.
    /// </summary>
    /// <param name="ammus">fysiikka objecti ammus</param>
    /// <param name="seina">fysiikka objecti seina</param>
    public void AmmusOsuiSeinaan(PhysicsObject ammus, PhysicsObject seina)
    {
        if (seina.Tag.ToString() == "reuna") 
        {
            ammus.Destroy();
        }
        ammus.Destroy();
    }

    /// <summary>
    /// Hoidellaan aseella ampuminen
    /// </summary>
    /// <param name="ase">Pelaajan ase</param>
    public void AmmuAseella(AssaultRifle ase)
    {
        PhysicsObject ammus = ase.Shoot();
        if (ammus != null)
        {
            ammus.Size *= 0.5;
            AddCollisionHandler<PhysicsObject, Zombi>(ammus, "zombi" , AmmusOsuiZombiin);
            AddCollisionHandler(ammus, "seina", AmmusOsuiSeinaan);
            AddCollisionHandler(ammus, "reuna", AmmusOsuiSeinaan);
            ammus.MaximumLifetime = TimeSpan.FromSeconds(1.0);
        }
    }

    /// <summary>
    /// Hoidetaan tähtääminen
    /// Määritetään uusi Vector suunta, johon lasketaan pelaajan aseen ja hiiren välinen suunta.
    /// </summary>
    /// <param name="hiirenliike">hiirenliike</param>
    public void Tahtaa(AnalogState hiirenliike)
    {
        //MessageDisplay.Add(Mouse.PositionOnWorld.X + "");
        if(pelaajaKatsooVasemmalle == false && Mouse.PositionOnScreen.X < 0)
        {
            pelaaja.MirrorImage();
            pelaajanAse.FlipImage();
            pelaajanAse.X = -10;
            pelaajaKatsooVasemmalle = true;
        }
        else if(pelaajaKatsooVasemmalle == true && Mouse.PositionOnScreen.X > 0)
        {
            pelaaja.MirrorImage();
            pelaajanAse.FlipImage();
            pelaajanAse.X = 10;
            pelaajaKatsooVasemmalle = false;
        }
        
        Vector suunta = (Mouse.PositionOnWorld - pelaajanAse.AbsolutePosition).Normalize();
        pelaajanAse.Angle = suunta.Angle;
    }

    #endregion

    #region Ohjaimet

    /// <summary>
    /// Asetetaan pelissä käytettävät ohjaimet.
    /// </summary>
    public void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.A, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta hahmoa vasemmalle", pelaaja, nopeusVasen);
        Keyboard.Listen(Key.A, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.D, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta hahmoa oikealle", pelaaja, nopeusOikea);
        Keyboard.Listen(Key.D, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.W, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta hahmoa ylös", pelaaja, nopeusYlos);
        Keyboard.Listen(Key.W, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.S, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta hahmoa alas", pelaaja, nopeusAlas);
        Keyboard.Listen(Key.S, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);

        Mouse.ListenMovement(0.1, Tahtaa, "Pelaaja 1: Tähtää aseella");
        Mouse.Listen(MouseButton.Left, ButtonState.Down, AmmuAseella, "Pelaaja1: Ammu aseella", pelaajanAse);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    /// <summary>
    /// Tarkastellaan meneekö pelaajat yli laitojen ja asetetaan pelaajien nopeus.
    /// </summary>
    /// <param name="pelaaja">Fysiikka objekti pelaaja</param>
    /// <param name="nopeus">Pelaajan nopeus</param>
    public void AsetaNopeus(PhysicsObject pelaaja, Vector nopeus)
    {
        if ((nopeus.Y > 0) && (pelaaja.Top > Level.Top))
        {
            pelaaja.Velocity = Vector.Zero;
            return;
        }
        if ((nopeus.Y < 0) && (pelaaja.Bottom < Level.Bottom))
        {
            pelaaja.Velocity = Vector.Zero;
            return;
        }
        if ((nopeus.X > 0) && (pelaaja.Right > Level.Right))
        {
            pelaaja.Velocity = Vector.Zero;
            return;
        }
        if ((nopeus.X < 0) && (pelaaja.Left < Level.Left))
        {
            pelaaja.Velocity = Vector.Zero;
            return;
        }

        pelaaja.Velocity = nopeus;

    }

    #endregion

    /// <summary>
    /// Pelin lopetus näyttö.
    /// Tyhjentää pelikentän.
    /// Tallentaa pelaajan nimen ja highscoren
    /// jos hän pääsee TOP 10 listalle.
    /// Palauttaa sen jälkeen takaisin päävalikkoon.
    /// </summary>
    public void GameOver()
    {
        ClearAll();
        if (pelaajaKuoli == true)
        {
            HighScoreWindow topIkkuna = new HighScoreWindow("Parhaat pisteet",
                                                        "Hävisit pelin, onnistui keräämään %p pistettä! Syötä nimesi: ",
                                                        topLista, pelaajanPisteet);
            topIkkuna.Closed += TallennaPisteet;
            Add(topIkkuna);
        }
        if (zombiLaskuri.Value == 0)
        {
            HighScoreWindow topIkkuna = new HighScoreWindow("Parhaat pisteet",
                                                        "Onneksi olkoon, Voitit pelin ja onnistui keräämään %p pistettä! Syötä nimesi: ",
                                                        topLista, pelaajanPisteet);
            topIkkuna.Closed += TallennaPisteet;
            Add(topIkkuna);
        }
    }
}