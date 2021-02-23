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
//           Pelaaja hahmo lähtee pyörimään liikuttuaan seinässä kiinni.
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

    // Luodaan zombeille attribuutit
    Zombi zombi1;
    Zombi zombi2;
    Zombi zombi3;
    Zombi zombi4;
    PathFollowerBrain polkuAivot1;
    PathFollowerBrain polkuAivot2;
    PathFollowerBrain polkuAivot3;
    PathFollowerBrain polkuAivot4;
    FollowerBrain seuraajaAivot1;
    FollowerBrain seuraajaAivot2;
    FollowerBrain seuraajaAivot3;
    FollowerBrain seuraajaAivot4;

    // Pelaajan pisteet
    IntMeter pelaajanPisteet;

    #endregion

    public override void Begin()
    {
        LuoKentta();
        AsetaOhjaimet();
        LisaaLaskurit();
        ZombiAjastin();
    }

    #region Laskuri ja ajastin

    public void LisaaLaskurit()
    {
        pelaajanPisteet = LuoPisteLaskuri(Screen.Left + 100.0, Screen.Top - 100.0);
    }

    IntMeter LuoPisteLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);

        Label naytto = new Label();
        naytto.BindTo(laskuri);
        naytto.X = x;
        naytto.Y = y;
        naytto.TextColor = Color.BloodRed;
        naytto.BorderColor = Level.BackgroundColor;
        naytto.Color = Level.BackgroundColor;
        Add(naytto);

        return laskuri;
    }

    /// <summary>
    /// Ajastaa zombien ilmestymiset kentän rajalle.
    /// </summary>
    public void ZombiAjastin()
    {
        // 30 sekunnin ajan luodaan zombi kahden sekunnin välein.
        Timer ajastin = new Timer();
        ajastin.Interval = 2.0;
        ajastin.Timeout += LuoZombi;
        ajastin.Start(15);

        // 30 sekunnin ajan luodaan zombi yhden sekunnin välein.
        ajastin.Interval = 1.0;
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
        pelaaja = LuoPelaaja(-200.0, 0.0);

        LuoRakennus();

        // Peelikentän ja ikkunan koko
        Level.Size = new Vector(1920, 1080);
        SetWindowSize(1920, 1080);

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

        Level.BackgroundColor = Color.Black;
        Camera.ZoomToLevel(); // zoomataan koko kentän alueelle.
    }
    #region Rakennus
    /// <summary>
    /// Luo pelin rakennuksen.
    /// </summary>
    public void LuoRakennus()
    {

        TileMap kentta = TileMap.FromLevelAsset("ShooterKentta");
        kentta.SetTileMethod('x', LuoSeinat);
        kentta.SetTileMethod('v', LuoIkkunat);
        kentta.SetTileMethod('l', LuoLattia);
        kentta.Execute(20, 20);
    }

    public void LuoSeinat(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject seina = new PhysicsObject(leveys, korkeus);
        seina.Position = paikka;
        //seina.Image = LoadImage("seina");
        seina.Tag = "seina";
        seina.MakeStatic();
        Add(seina);
    }

    public void LuoIkkunat(Vector paikka, double leveys, double korkeus)
    {
        GameObject ikkuna = new GameObject(leveys, korkeus);
        ikkuna.Position = paikka;
        ikkuna.Color = Color.Aqua;
        Add(ikkuna);
    }

    public void LuoLattia(Vector paikka, double leveys, double korkeus)
    {
        GameObject lattia = new GameObject(leveys, korkeus);
        lattia.Position = paikka;
        //lattia.Image = LoadImage("lattia");
        lattia.Color = Level.BackgroundColor;
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
        pelaaja.Color = Color.DarkBlue;
        pelaaja.X = x;
        pelaaja.Y = y;
        pelaaja.Restitution = 0.0;
        pelaaja.StopAngular(); // pysäyttää pelaajahahmon pyörimisen.
        pelaaja.Tag = "pelaaja";
        //pelaaja.Image = LoadImage("pelaaja");
        Add(pelaaja, 1);
        AddCollisionHandler<PhysicsObject, Zombi>(pelaaja, ZombiOsuuPelaajaan);

        pelaajanAse = new AssaultRifle(30, 10);
        //pelaajanAse.Ammo.Value = 100; // Ammusten määrä
        pelaajanAse.InfiniteAmmo = true;
        pelaajanAse.AttackSound = null;
        pelaajanAse.CanHitOwner = false;
        pelaaja.Add(pelaajanAse);

        return pelaaja;
    }

    #endregion

    #region Zombi

    /// <summary>
    /// DevTool luo zombi
    /// poistetaan pelistä, kun ei enää tarvitse
    /// </summary>
    public void LuoZombi()
    {
        zombi1 = LuoZombi(0.0, Level.Bottom + 20); // alareuna spawni
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
        //zombi.Image = LoadImage("zombi");
        Add(zombi, 1);

        return zombi;
    }

    public void ZombiOsuuPelaajaan(PhysicsObject pelaaja, Zombi kohde)
    {
        pelaaja.Destroy();
        GameOver();
    }

    public List<Vector> ZombinReittiOikea()
    {
        List<Vector> polku = new List<Vector>();
        polku.Add(new Vector(Level.Right -450, 20.0));

        return polku;
    }
    public List<Vector> ZombinReittiVasen()
    {
        List<Vector> polku = new List<Vector>();
        polku.Add(new Vector(Level.Left + 700.0, 20.0));

        return polku;
    }
    public List<Vector> ZombinReittiYla()
    {
        List<Vector> polku = new List<Vector>();
        polku.Add(new Vector(-185, Level.Top - 350));
        polku.Add(new Vector(-185, Level.Top - 550));
        return polku;
    }
    public List<Vector> ZombinReittiAla()
    {
        List<Vector> polku = new List<Vector>();
        polku.Add(new Vector(-50, Level.Bottom + 350));
        polku.Add(new Vector(-150, Level.Bottom + 360));



        return polku;
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
            kohde.OtaVastaanOsuma();
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

    public void GameOver()
    {
        Label peliLoppu = new Label(500.0, 500.0);
        peliLoppu.X = 0.0;
        peliLoppu.Y = 0.0;
        peliLoppu.Text = "Hävisit pelin. \n \n" + "Onnistuit keräämään: " + pelaajanPisteet + " pistettä.\n \n Paina Esc-näppäintä lopettaaksesi.";
        peliLoppu.Color = Level.BackgroundColor;
        peliLoppu.TextColor = Color.Red;
        peliLoppu.BorderColor = Color.Red;
        Add(peliLoppu);
        //MessageDisplay.Add("Hävisit pelin!");
        //MessageDisplay.Add("Pisteesi: " + pelaajanPisteet);
    }
}