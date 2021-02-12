using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
/// @author Samuli Juutinen
/// @version 12.02.2021
/// <summary>
/// Ylhäältäpäin kuvattu zombie räiskintä peli.
/// </summary>
public class ZombiPeli : PhysicsGame
{
    #region Atribuutit

    // Luodaan aseen atribuutti.
    AssaultRifle pelaajan1Ase;

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
    PhysicsObject pelaaja1;
    //PhysicsObject pelaaja2;

    #endregion

    public override void Begin()
    {
        LuoKentta();
        AloitaPeli();
        AsetaOhjaimet();

        
    }

    public void AloitaPeli()
    {
        pelaaja1 = LuoPelaaja(-200.0, 0.0);
        pelaajan1Ase = new AssaultRifle(30, 10);
        //pelaajan1Ase.Ammo.Value = 100; // Ammusten määrä
        pelaajan1Ase.ProjectileCollision = AmmusOsui;
        pelaajan1Ase.Power.Value = 2000;
        pelaajan1Ase.CanHitOwner = false;
        pelaaja1.Add(pelaajan1Ase);

        // pelaaja2 = LuoPelaaja(200.0, 0.0);
    }

    /// <summary>
    /// Luodaan pelikenttä.
    /// Rajat ja kamera asetukset.
    /// </summary>
    public void LuoKentta()
    {
        LuoRakennus();

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
        LuoSeinat(10.0, 350.0, -400, -70);
        LuoSeinat(10.0, 150.0, -200, -170);
        LuoSeinat(200.0, 10.0, -300, -240);

        // Oikeasiipi
        LuoSeinat(200.0, 10, 300, -100);
        LuoSeinat(10.0, 350, 400, 70);
        LuoSeinat(10.0, 150.0, 200, 170);
        LuoSeinat(200.0, 10.0, 300, 240);
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
        Add(seina);
    }

    #endregion

    PhysicsObject LuoPelaaja(double x, double y)
    {
        PhysicsObject pelaaja = new PhysicsObject(25.0, 25.0);
        pelaaja.Shape = Shape.Circle;
        pelaaja.Color = Color.DarkBlue;
        pelaaja.X = x;
        pelaaja.Y = y;
        pelaaja.Restitution = 0.0;
        Add(pelaaja);
        return pelaaja;
    }

    #region Ase

    /// <summary>
    /// Hoidellaan ammuksen osuminen
    /// </summary>
    /// <param name="ammus">ammus</param>
    /// <param name="kohde">zombi</param>
    public void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
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
            // ammus.Image = ...
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
        Vector suunta = (Mouse.PositionOnWorld - pelaajan1Ase.AbsolutePosition).Normalize();
        pelaajan1Ase.Angle = suunta.Angle;
    }

    #endregion

    #region Ohjaimet

    /// <summary>
    /// Asetetaan pelissä käytettävät ohjaimet.
    /// </summary>
    public void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.A, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta hahmoa vasemmalle", pelaaja1, nopeusVasen);
        Keyboard.Listen(Key.A, ButtonState.Released, AsetaNopeus, null, pelaaja1, Vector.Zero);
        Keyboard.Listen(Key.D, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta hahmoa oikealle", pelaaja1, nopeusOikea);
        Keyboard.Listen(Key.D, ButtonState.Released, AsetaNopeus, null, pelaaja1, Vector.Zero);
        Keyboard.Listen(Key.W, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta hahmoa ylös", pelaaja1, nopeusYlos);
        Keyboard.Listen(Key.W, ButtonState.Released, AsetaNopeus, null, pelaaja1, Vector.Zero);
        Keyboard.Listen(Key.S, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta hahmoa alas", pelaaja1, nopeusAlas);
        Keyboard.Listen(Key.S, ButtonState.Released, AsetaNopeus, null, pelaaja1, Vector.Zero);

        Mouse.ListenMovement(0.1, Tahtaa, "Pelaaja 1: Tähtää aseella");
        Mouse.Listen(MouseButton.Left, ButtonState.Down, AmmuAseella, "Pelaaja1: Ammu aseella", pelaajan1Ase);

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
}

#endregion



