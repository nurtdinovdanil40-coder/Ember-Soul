using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace game;

public class Game1 : Core
{
    private int batHealth = 3;
    private int batMaxHealth = 3;
    private Rectangle attackHitbox;
    private float attackHitboxTimer = 0.5f;
    private const float ATTACK_HITBOX_DURATION = 0.15f;
    private float flashTimer = 0f;
    private int heroHealth = 3;
    private int heroMaxHealth = 3;
    private float invincibilityTimer = 0f;
    private float invicibiliDuration = 1.0f;
    private Vector2 visualOffset = Vector2.Zero;
    private Texture2D pixelTexture;
    private bool showHitboxes = true;
    private AnimatedSprite hero;
    private AnimatedSprite bat;
    private Texture2D logo;
    private Vector2 heroPosition;
    private SpriteEffects heroEffect;
    private Vector2 batPosition;
    private const float speed = 5.0f;
    private Vector2 batVelocity;
    private Tilemap tilemap;
    private Rectangle roomBounds;
    private Dictionary<string, Animation> animations;
    private string currentAnimation;
    private Vector2 _attackOffset = Vector2.Zero;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float attackDuration = 0.5f;
    private bool canAttack = true;
    private const float HERO_HITBOX_RADIUS = 49f;
    private Vector2 pivot = Vector2.Zero;
    public Game1() : base("game", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {
        base.Initialize();

        Rectangle screenBounds = GraphicsDevice.PresentationParameters.Bounds;

        roomBounds = new Rectangle(
            (int)tilemap.TileWidth,
            (int)tilemap.TileHeight,
            screenBounds.Width - (int)tilemap.TileWidth * 2,
            screenBounds.Height - (int)tilemap.TileHeight * 2
        );

        int centerRow = tilemap.Rows / 2;
        int centerColumn = tilemap.Columns / 2;
        heroPosition = new Vector2(centerColumn * tilemap.TileWidth, centerRow * tilemap.TileHeight);

        batPosition = new Vector2(roomBounds.Left, roomBounds.Top);

        AssignRandomBatVelocity();
        
        pixelTexture = new Texture2D(GraphicsDevice,1,1);
        pixelTexture.SetData(new[] { Color.White });
    }

    protected override void LoadContent()
    {
        logo = Content.Load<Texture2D>("images/ShadowFiend");


        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
        TextureAtlas idleAtlas = TextureAtlas.FromFile(Content, "images/Knight_idle.xml");
        TextureAtlas walkAtlas = TextureAtlas.FromFile(Content, "images/Knight_walk.xml");
        TextureAtlas attackAtlas = TextureAtlas.FromFile(Content, "images/Knight_attack.xml");

        bat = atlas.CreateAnimatedSprite("bat-animation");
        bat.Scale = new Vector2(4.0f, 4.0f);

        tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
        tilemap.Scale = new Vector2(4.0f, 4.0f);

        animations = new Dictionary<string, Animation>
        {
            {"idle", idleAtlas.GetAnimation("knight-idle")},
            {"walk", walkAtlas.GetAnimation("knight-walk")},
            {"attack", attackAtlas.GetAnimation("knight-attack")}
        }; 

        hero = new AnimatedSprite(animations["idle"]);
        hero.Scale = new Vector2(2.5f, 2.5f);
        currentAnimation = "idle";
        pivot = new Vector2(hero.Width/2, hero.Height/2);
    }

    protected override void Update(GameTime gameTime)
    {
        hero.Update(gameTime);
        bat.Update(gameTime);

        checkInput();
        if(flashTimer > 0)
        {
            flashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        if (isAttacking)
        {
            attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (attackTimer <= 0f)
            {
                isAttacking = false;
                canAttack = true;
                visualOffset = Vector2.Zero;
                SetAnimation("idle");
            }
            else
            {
                 Rectangle batRect = new Rectangle(
            (int)batPosition.X,
            (int)batPosition.Y,
            (int)bat.Width,
            (int)bat.Height
            );
            
            if (attackHitbox.Intersects(batRect) && batHealth > 0)
            {
                batHealth--;
                Console.WriteLine($"Bat take damage: {batHealth}/{batMaxHealth}");
                
                Vector2 direction = batPosition - heroPosition;
                direction.Normalize();
                batPosition = heroPosition + direction * 80;
                
                isAttacking = false;
                canAttack = true;
                if (batHealth <= 0)
                {
                    Console.WriteLine("Bat dead");
                    batHealth = batMaxHealth;
                    AssignRandomBatVelocity();
                }
                }
            }

            }

        Rectangle screenBounds = new Rectangle(
            0,
            0,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight
        );

        Circle heroBounds = new Circle(
            (int)heroPosition.X,
            (int)heroPosition.Y,
            (int)HERO_HITBOX_RADIUS
        );

        if(heroBounds.Left < roomBounds.Left)
        {
            heroPosition.X = roomBounds.Left;
        }
        else if (heroBounds.Right > roomBounds.Right)
        {
            heroPosition.X = roomBounds.Right - hero.Width;
        }
        
        if (heroBounds.Top < roomBounds.Top)
        {
            heroPosition.Y = roomBounds.Top;
        }
        else if (heroBounds.Bottom > roomBounds.Bottom)
        {
            heroPosition.Y = roomBounds.Bottom - hero.Height;
        }

        Vector2 newBatPosition = batPosition + batVelocity;

        Circle batBounds = new Circle(
            (int)(batPosition.X + bat.Width / 2),
            (int)(batPosition.Y + bat.Height / 2),
            (int)(bat.Width / 2)
        );

        Vector2 normal = Vector2.Zero;

        if(batBounds.Left < roomBounds.Left)
        {
            normal.X = Vector2.UnitX.X;
            newBatPosition.X = roomBounds.Left;
        }
        else if(batBounds.Right > roomBounds.Right)
        {
            normal.X = -Vector2.UnitX.X;
            newBatPosition.X = roomBounds.Right - bat.Width;
        }
        
        if (batBounds.Top < roomBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y;
            newBatPosition.Y = roomBounds.Top;
        }
        else if (batBounds.Bottom > roomBounds.Bottom)
        {
            normal.Y = -Vector2.UnitY.Y;
            newBatPosition.Y = roomBounds.Bottom - bat.Height;
        }

        if (normal != Vector2.Zero)
        {
            normal.Normalize();
            batVelocity = Vector2.Reflect(batVelocity, normal);
        }

        batPosition = newBatPosition;

        if (heroBounds.Intersects(batBounds))
        {
            TakeDamage(1);
            
            int column = Random.Shared.Next(1, tilemap.Columns - 1);
            int row = Random.Shared.Next(1, tilemap.Rows - 1);
            batPosition = new Vector2(column * bat.Width, row * bat.Height);
            AssignRandomBatVelocity();
        }
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {

        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        tilemap.Draw(SpriteBatch);
        hero.Draw(SpriteBatch, heroPosition + visualOffset, pivot, heroEffect);
        bat.Draw(SpriteBatch, batPosition, Vector2.Zero, SpriteEffects.None);
        if (flashTimer > 0f)
        {
            hero.Color = Color.Red;
        }
        else
            hero.Color = Color.White;
        if (showHitboxes)
        {
            if (isAttacking)
            {
                DrawHitbox(attackHitbox, Color.Orange);
            }
            Circle heroBounds = new Circle(
                (int)heroPosition.X,      // ← центр такой же, как в Update
                (int)heroPosition.Y,
                (int)HERO_HITBOX_RADIUS   // ← радиус такой же, как в Update
            );
            DrawHitbox(heroBounds, Color.Red);
            
            Circle batBounds = new Circle(
                (int)(batPosition.X + bat.Width / 2),
                (int)(batPosition.Y + bat.Height / 2),
                (int)(bat.Width / 2)
            );
            DrawHitbox(batBounds, Color.Green);
        }
                
        int barWidth = 200;
        int barHeight = 20;
        int barX = 50;
        int barY = 25;

        
        Rectangle backRect = new Rectangle(barX, barY, barWidth, barHeight);
        SpriteBatch.Draw(pixelTexture, backRect, Color.Gray);

        
        int healthWidth = (int)((float)heroHealth / heroMaxHealth * barWidth);
        Rectangle healthRect = new Rectangle(barX, barY, healthWidth, barHeight);
        SpriteBatch.Draw(pixelTexture, healthRect, Color.Red);

        SpriteBatch.Draw(pixelTexture, new Rectangle((int)heroPosition.X - 3, (int)heroPosition.Y - 3, 6, 6), Color.Red);
        SpriteBatch.End();

        base.Draw(gameTime);
    }

    private void checkInput()
    {
        float movspeed = speed;

        bool isMoving = false;
        if (Input.Mouse.WasButtonJustPressed(MouseInfo.MouseButton.Left) && canAttack)
        {
            attackTimer = attackDuration;
            isAttacking = true;
            canAttack = false;
            SetAnimation("attack");
            
            if (heroEffect == SpriteEffects.FlipHorizontally)
                visualOffset = new Vector2(-30, 0); 
            else
                visualOffset = new Vector2(30, 0); 
            isAttacking = true;
            attackHitboxTimer = ATTACK_HITBOX_DURATION;
            
            int attackRange = 10;
            int attackWidth = 100;
            int attackHeight = 150;
            
            if (heroEffect == SpriteEffects.FlipHorizontally)
            {
                attackHitbox = new Rectangle(
                    (int)heroPosition.X - attackRange - attackWidth,
                    (int)heroPosition.Y - attackHeight / 2,
                    attackWidth,
                    attackHeight
                );
            }
            else
                attackHitbox = new Rectangle(
                    (int)heroPosition.X + attackRange,
                    (int)heroPosition.Y - attackHeight / 2,
                    attackWidth,
                    attackHeight
                );
        }
        
        if (Input.Keyboard.IsKeyDown(Keys.Space))
        {
            movspeed *= 1.5f;
        }
        if (Input.Keyboard.IsKeyDown(Keys.W))
        {
            heroPosition.Y -= movspeed;
            isMoving = true;
        }
        if (Input.Keyboard.IsKeyDown(Keys.A))
        {
            heroPosition.X -= movspeed;
            heroEffect = SpriteEffects.FlipHorizontally;
            isMoving = true;
        }
        if (Input.Keyboard.IsKeyDown(Keys.S))
        {
            heroPosition.Y += movspeed;
            isMoving = true;
        }
        if (Input.Keyboard.IsKeyDown(Keys.D))
        {
            heroPosition.X += movspeed;
            heroEffect = SpriteEffects.None;
            isMoving = true;
        }
        if (!isAttacking)
        {
            if (isMoving)
            {
                SetAnimation("walk");
            }
            else
            {
                SetAnimation("idle");
            }
        }
    }

    private void AssignRandomBatVelocity()
    {
        float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

        float x = (float)Math.Cos(angle);
        float y = (float)Math.Sin(angle);
        Vector2 direction = new Vector2(x,y);

        batVelocity = direction * speed;
    }
    private async Task SetAnimation(string animationName)
    {
        if (currentAnimation == animationName) return;
        
        Vector2 oldPosition = heroPosition;
        
        hero.Animation = animations[animationName];
        currentAnimation = animationName;
        
        
        pivot = new Vector2(hero.Width/4, hero.Height /5);
    }
    private void DrawHitbox(Rectangle bounds, Color color)
    {
        int borderWidth = 2;
        Rectangle top = new Rectangle(bounds.X, bounds.Y, bounds.Width, borderWidth);
        Rectangle bottom = new Rectangle(bounds.X, bounds.Y + bounds.Height - borderWidth, bounds.Width, borderWidth);
        Rectangle left = new Rectangle(bounds.X, bounds.Y, borderWidth, bounds.Height);
        Rectangle right = new Rectangle(bounds.X + bounds.Width - borderWidth, bounds.Y, borderWidth, bounds.Height);
        
        SpriteBatch.Draw(pixelTexture, top, color);
        SpriteBatch.Draw(pixelTexture, bottom, color);
        SpriteBatch.Draw(pixelTexture, left, color);
        SpriteBatch.Draw(pixelTexture, right, color);
    }

    private void DrawHitbox(Circle circle, Color color)
    {

        Rectangle bounds = new Rectangle(
            circle.X - circle.Radius,   
            circle.Y - circle.Radius,  
            circle.Radius * 2,          
            circle.Radius * 2      
        );
        DrawHitbox(bounds, color);
    }
    private void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0f) return;
        
        heroHealth -= damage;
        invincibilityTimer = invicibiliDuration;
        flashTimer = 0.5f;

        if(heroHealth <= 0)
        {
            heroHealth = 0;
            Die();
        }
    }
    private void Die()
    {
        Console.WriteLine("Death");
        ResetGame();
    }
    private void ResetGame()
    {
        heroHealth = heroMaxHealth;
        invincibilityTimer = 0f;
        
        int centerRow = tilemap.Rows / 2;
        int centerColumn = tilemap.Columns / 2;
        heroPosition = new Vector2(centerColumn * tilemap.TileWidth, centerRow * tilemap.TileHeight);
        
        
        batPosition = new Vector2(roomBounds.Left, roomBounds.Top);
        AssignRandomBatVelocity();
    }

}