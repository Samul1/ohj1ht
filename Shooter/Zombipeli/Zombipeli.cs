using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
/// @author Samuli Juutinen
/// @version 16.02.2021
/// <summary>
/// Ylhäältäpäin kuvattu zombie räiskintä peli.
/// </summary>
/// 
// Ongelma: Jos kentällä on enemmän kuin yksi zombi niin edelliset zombit eivät voi tuhoutua.
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
    Zombi zombi;

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

    public void ZombiAjastin()
    {
        Timer.CreateAndStart(1.5, LuoZombi);
    }

    #region Laskuri

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


    #endregion

    #region Pelikenttan ja pelaajan luonti.

    /// <summary>
    /// Luodaan pelikenttä.
    /// Rajat ja kamera asetukset.
    /// </summary>
    public void LuoKentta()
    {
        pelaaja = LuoPelaaja(-200.0, 0.0);
        zombi = LuoZombi(0, 0);

        LuoRakennus();

        // Peelikentän ja ikkunan koko
        Level.Size = new Vector(1920, 1080);
        SetWindowSize(1920, 1080);

        // Asetetaan kentän rajat
        vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = 1.0;
        vasenReuna.IsVisible = false;

        oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.IsVisible = false;

        alaReuna = Level.CreateBottomBorder();
        alaReuna.Restitution = 1.0;
        alaReuna.IsVisible = false;

        ylaReuna = Level.CreateTopBorder();
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
        // Keskiosa
        LuoSeinat(400.0, 10, 0, 100);
        LuoSeinat(400.0, 10, 0, -100);

        // Vasensiipi
        LuoSeinat(200.0, 10, -300, 100);
        LuoSeinat(10.0, 350.0, -405, -70);
        LuoSeinat(10.0, 150.0, -205, -170);
        LuoSeinat(190.0, 10.0, -305, -240);

        // Oikeasiipi
        LuoSeinat(200.0, 10, 300, -100);
        LuoSeinat(10.0, 350, 405, 70);
        LuoSeinat(10.0, 150.0, 205, 170);
        LuoSeinat(190.0, 10.0, 305, 240);
    }

    /// <summary>
    /// Luo rakennuksen seinät
    /// </summary>
    /// <param name="leveys">Seinän leveys</param>
    /// <param name="korkeus">Seinän korkeus</param>
    /// <param name="x">seinän x-koordinaatti</param>
    /// <param name="y">seinän y-koordinaatti</param>
    public void LuoSeinat(double leveys, double korkeus, double x, double y)
    {
        PhysicsObject seina = PhysicsObject.CreateStaticObject(leveys, korkeus);
        seina.Color = Color.Gray;
        seina.Restitution = 0.0;
        seina.X = x;
        seina.Y = y;
        seina.Tag = "seina";
        Add(seina);
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
        PhysicsObject pelaaja = PhysicsObject.CreateStaticObject(25.0, 25.0);
        pelaaja.Shape = Shape.Circle;
        pelaaja.Color = Color.DarkBlue;
        pelaaja.X = x;
        pelaaja.Y = y;
        pelaaja.Restitution = 0.0;
        Add(pelaaja);
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
        zombi = LuoZombi(0.0, 0.0);
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
        Add(zombi);
        return zombi;
    }

    public void ZombiOsuuPelaajaan(PhysicsObject pelaaja1, Zombi kohde)
    {
        pelaaja1.Destroy();
        
        MessageDisplay.Add("Hävisit pelin!");
        MessageDisplay.Add("Pisteesi: " + pelaajanPisteet);
    }

    #endregion

    #region Aseen kayttaytyminen

    /// <summary>
    /// Hoidellaan ammuksen osuminen
    /// </summary>
    /// <param name="ammus">ammus</param>
    /// <param name="kohde">zombi</param>
    public void AmmusOsui(PhysicsObject ammus, Zombi kohde)
    {
        if (kohde == zombi)
        {
            pelaajanPisteet.Value += 1;
            kohde.OtaVastaanOsuma();
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
            AddCollisionHandler<PhysicsObject, Zombi>(ammus, AmmusOsui);
            ammus.MaximumLifetime = TimeSpan.FromSeconds(0.5);
        }
    }

    /// <summary>
    /// Hoidetaan tähtääminen
    /// Määritetään uusi Vector suunta, johon lasketaan pelaajan aseen ja hiiren välinen suunta.
    /// </summary>
    /// <param name="hiirenliike">hiirenliike</param>
    public void Tahtaa(AnalogState hiirenliike)
    {
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

        Keyboard.Listen(Key.Space, ButtonState.Pressed, LuoZombi, "Luo zombin kentälle"); // poistetaan pelistä, kun ei enää testauksessa.

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
}