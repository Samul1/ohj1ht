using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
/// @author Samuli Juutinen
/// @version 10.02.2021
/// <summary>
/// Ylhäältäpäin kuvattu zombie räiskintä peli.
/// </summary>
// HUOM:
// Tämä projekti käyttää hyvin kokeellista FarseerPhysics -fysiikkamoottoria
// Varmista että projektin paketit ovat aina uusimmassa versiossa.
// Katso myös https://tim.jyu.fi/view/kurssit/jypeli/farseer
// kaikista tämän version eroavaisuuksista sekä tunnetuista ongelmista. Täydennä listaa tarvittaessa.

public class Shooter : PhysicsGame
{
    #region Atribuutit

    // Luodaan uudet Vectorit pelaajan liikuttamista varten.
    Vector nopeusYlos = new Vector(0, 500);
    Vector nopeusAlas = new Vector(0, -500);
    Vector nopeusVasen = new Vector(-500, 0);
    Vector nopeusOikea = new Vector(500, 0);

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
        // pelaaja2 = LuoPelaaja(200.0, 0.0);
    }

    PhysicsObject LuoPelaaja(double x, double y)
    {
        PhysicsObject pelaaja = new PhysicsObject(50.0, 50.0);
        pelaaja.Shape = Shape.Circle;
        pelaaja.Color = Color.DarkBlue;
        pelaaja.X = x;
        pelaaja.Y = y;
        pelaaja.Restitution = 0.0;
        Add(pelaaja);
        return pelaaja;
    }

    public void LuoKentta()
    {

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


        Camera.ZoomToLevel(); // zoomataan koko kentän alueelle.
    }

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