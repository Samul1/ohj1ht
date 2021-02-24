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
    Vector nopeusYlos = new Vector(0, 200);
    Vector nopeusAlas = new Vector(0, -200);
    Vector nopeusVasen = new Vector(-200, 0);
    Vector nopeusOikea = new Vector(200, 0);

    // Kentän rajat
    PhysicsObject vasenReuna;
    PhysicsObject oikeaReuna;
    PhysicsObject ylaReuna;
    PhysicsObject alaReuna;

    // Luodaan pelaaja attribuutti luokan sisällä
    PhysicsObject pelaaja;
    // Luodaan aseen atribuutti.
    AssaultRifle pelaajanAse;
    // Tarkastetaan onko pelaaja hengissä
    bool pelaajaKuoli = false;

    // Luodaan zombeille attribuutit
    Zombi zombi1;
    Zombi zombi2;
    Zombi zombi3;
    Zombi zombi4;
    PathFollowerBrain polkuAivot1;
    FollowerBrain seuraajaAivot1;
    PathFollowerBrain polkuAivot2;
    FollowerBrain seuraajaAivot2;
    PathFollowerBrain polkuAivot3;
    FollowerBrain seuraajaAivot3;
    PathFollowerBrain polkuAivot4;
    FollowerBrain seuraajaAivot4;

    // Laskurit
    IntMeter pelaajanPisteet;
    IntMeter zombiLaskuri;

    // HighScore ikkuna
    ScoreList topLista = new ScoreList(10, false, 0);

    // Rakennuksen palikoiden koko
    const double RUUDUN_LEVEYS = 25;
    const double RUUDUN_KORKEUS = 25;

    #endregion

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
        //Camera.ZoomToLevel(); // zoomataan koko kentän alueelle.
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
        zombi1 = LuoZombi(RandomGen.NextDouble(-200, 200), Level.Bottom + 20); // alareuna spawni
        Zombi1PerusAivot();

        zombi2 = LuoZombi(RandomGen.NextDouble(-200, 200), Level.Top - 20); // yläreuna spawni
        Zombi2PerusAivot();

        zombi3 = LuoZombi(Level.Left + 50, RandomGen.NextDouble(-200, 200)); // vasenreuna spawni
        Zombi3PerusAivot();

        zombi4 = LuoZombi(Level.Right - 50, RandomGen.NextDouble(-200, 200)); // oikeareuna spawni
        Zombi4PerusAivot();


    }

    /// <summary>
    /// Luo zombin
    /// </summary>
    /// <param name="x">zombin x-koordinaatit</param>
    /// <param name="y">zombin y-koordinaatit</param>
    /// <returns></returns>
    Zombi LuoZombi(double x, double y)
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

    /// <summary>
    /// Oikealle spawnaavien sombien perusreitti.
    /// </summary>
    /// <returns>polku</returns>
    public List<Vector> ZombinReittiOikea()
    {
        List<Vector> polku = new List<Vector>();
        polku.Add(new Vector(Level.Right - 450, 20.0));

        return polku;
    }

    /// <summary>
    /// Vasemmalle spawnaavien zombien perusreitti.
    /// </summary>
    /// <returns>polku</returns>
    public List<Vector> ZombinReittiVasen()
    {
        List<Vector> polku = new List<Vector>();
        polku.Add(new Vector(Level.Left + 700.0, 20.0));

        return polku;
    }

    /// <summary>
    /// Ylhäälle spawnaavien zombien perusreitti.
    /// </summary>
    /// <returns>polku</returns>
    public List<Vector> ZombinReittiYla()
    {
        List<Vector> polku = new List<Vector>();
        polku.Add(new Vector(-185, Level.Top - 350));
        polku.Add(new Vector(-185, Level.Top - 550));
        return polku;
    }

    /// <summary>
    /// Alhaalle spawnaavien zombien perusreitti.
    /// </summary>
    /// <returns>polku</returns>
    public List<Vector> ZombinReittiAla()
    {
        List<Vector> polku = new List<Vector>();
        polku.Add(new Vector(-50, Level.Bottom + 350));
        polku.Add(new Vector(-150, Level.Bottom + 360));



        return polku;
    }

    /// <summary>
    /// Zombi1:n aivot.
    /// Liikkuu kentän alareunasta talon sisälle
    /// ja lähtee jahtaamaan pelaajaa, pelaajan 
    /// tullessa tarpeeksi lähelle
    /// </summary>
    public void Zombi1PerusAivot()
    {
        polkuAivot1 = new PathFollowerBrain(50);
        zombi1.Brain = seuraajaAivot1;
        polkuAivot1.Path = ZombinReittiAla();
        seuraajaAivot1 = new FollowerBrain(pelaaja);
        seuraajaAivot1.Speed = 100;
        seuraajaAivot1.DistanceFar = 400;
        seuraajaAivot1.DistanceClose = 50;
        seuraajaAivot1.FarBrain = polkuAivot1;
        seuraajaAivot1.StopWhenTargetClose = false;
        seuraajaAivot1.TargetClose += ZombiLahellaPelaajaa;
    }

    /// <summary>
    /// Zombi2:n aivot.
    /// Liikkuu kentän alareunasta talon sisälle
    /// ja lähtee jahtaamaan pelaajaa, pelaajan 
    /// tullessa tarpeeksi lähelle
    /// </summary>
    public void Zombi2PerusAivot()
    {
        zombi2 = LuoZombi(0.0, Level.Top - 20); // yläreuna spawni
        polkuAivot2 = new PathFollowerBrain(50);
        zombi2.Brain = seuraajaAivot2;
        polkuAivot2.Path = ZombinReittiYla();
        seuraajaAivot2 = new FollowerBrain(pelaaja);
        seuraajaAivot2.Speed = 100;
        seuraajaAivot2.DistanceFar = 400;
        seuraajaAivot2.DistanceClose = 50;
        seuraajaAivot2.FarBrain = polkuAivot2;
        seuraajaAivot2.StopWhenTargetClose = false;
        seuraajaAivot2.TargetClose += ZombiLahellaPelaajaa;
    }

    /// <summary>
    /// Zombi3:n aivot.
    /// Liikkuu kentän alareunasta talon sisälle
    /// ja lähtee jahtaamaan pelaajaa, pelaajan 
    /// tullessa tarpeeksi lähelle
    /// </summary>
    public void Zombi3PerusAivot()
    {
        zombi3 = LuoZombi(Level.Left + 50, 0.0); // vasenreuna spawni
        polkuAivot3 = new PathFollowerBrain(50);
        zombi3.Brain = seuraajaAivot3;
        polkuAivot3.Path = ZombinReittiVasen();
        seuraajaAivot3 = new FollowerBrain(pelaaja);
        seuraajaAivot3.Speed = 100;
        seuraajaAivot3.DistanceFar = 400;
        seuraajaAivot3.DistanceClose = 50;
        seuraajaAivot3.FarBrain = polkuAivot3;
        seuraajaAivot3.StopWhenTargetClose = false;
        seuraajaAivot3.TargetClose += ZombiLahellaPelaajaa;
    }

    /// <summary>
    /// Zombi4:n aivot.
    /// Liikkuu kentän alareunasta talon sisälle
    /// ja lähtee jahtaamaan pelaajaa, pelaajan 
    /// tullessa tarpeeksi lähelle
    /// </summary>
    public void Zombi4PerusAivot()
    {
        zombi4 = LuoZombi(Level.Right - 50, 0.0); // oikeareuna spawni
        polkuAivot4 = new PathFollowerBrain(50);
        zombi4.Brain = seuraajaAivot4;
        polkuAivot4.Path = ZombinReittiOikea();
        seuraajaAivot4 = new FollowerBrain(pelaaja);
        seuraajaAivot2.Speed = 100;
        seuraajaAivot4.DistanceFar = 400;
        seuraajaAivot4.DistanceClose = 50;
        seuraajaAivot4.FarBrain = polkuAivot4;
        seuraajaAivot4.StopWhenTargetClose = false;
        seuraajaAivot4.TargetClose += ZombiLahellaPelaajaa;
    }

    public void ZombiLahellaPelaajaa()
    {
        //PlaySound("ZombiAani01");
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
        Vector suunta = (Mouse.PositionOnWorld - pelaaja.AbsolutePosition).Normalize();
        pelaaja.Angle = suunta.Angle;
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