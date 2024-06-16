
using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour

{   //Plus utile ================================
    //Initialisation d'une valeur booleene pour savoir si le joueur est sur le sol ou non
    //private bool _grounded;
    //ON remplace la valeur booleene par une methode 
    
    //old code for the OnCollisionEnter2D method, replaced on th 16/06/2024
    //if (other.gameObject.tag == "Ground"){
    //  _grounded = true;
    // }  
    
    //old code for jump
    //_grounded = false; //met la valeur de _grounded a false ce qui influence le flow de logique et provoque l'animation en ligne 62
    //_grounded n'est plus utile
    
    //=============================================
    
    
    //SerializeField permet de rajouter des options dans l'editeur Unity, ici on le fait pour speed
    [SerializeField]private float speed;
    //Initialisation du jumpPower
    [SerializeField]private float jumpPower;
    //Initialisation du wallJumpPower
    [SerializeField]private float horizontalWalljumpPower;
    //Initialisation du wallJumpPower
    [SerializeField]private float verticalWalljumpPower;
    //Initialisation d'un layer pour le sol
    [SerializeField]private LayerMask groundLayer;
    
    //Initialisation d'un layer pour le mur
    [SerializeField]private LayerMask wallLayer;
    
    //Vitesse pour le wall slide 
    [SerializeField]private float _wallSlideSpeed;
    
    //Initialisation d'un objet RigidBody sur lequel on va faire des modifications
    //Un objet RigidBody est utile car c'est en gros un corps physique avec une masse
    //Cette masse forme de base un poids et permet donc de simuler la gravite
    private Rigidbody2D _body;
    
    //Initialisation d'un animator
    private Animator _anim;
    
    //Initialisation d'un boxcollider
    private BoxCollider2D _boxCollider;
    
    //cooldown pour le wall jump
    private float _wallJumpCooldown;
    
    //Initialisation d'un horizontalInput qui est maintenant une variable globale
    private float _horizontalInput;

    
    //awake se produit lorsque le script est charge
    private void Awake() {
        //Recupere les references pour l'animator et le rigidbody du joueur
        _body = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>();
    }
    
    
    //cette methode update est mise a jour chaque frame et check pour des conditions qui 
    //influencent le jeu
    private void Update()
    {
        //_horizontalInput vaut une valeur entre -1 et 1 et permet donc de savoir si on va
        // a gauche ou a droite selon la valeur (-1=gauche et 1=gauche)
        _horizontalInput = Input.GetAxis("Horizontal");
        
        
        //Ces conditions permettent de flip le personnage de gauche a droite selon la valeure 
        //_horizontalInput
        if (_horizontalInput > 0.01f) {
            transform.localScale = Vector3.one;
        }
        else if(_horizontalInput < -0.01f) {
            transform.localScale = new Vector3(-1,1,1);
        }
        
        //Changement des parametres de l'animator, permet de changer les animations
        _anim.SetBool("run", _horizontalInput !=0);
        _anim.SetBool("grounded", isGrounded());
        
        //check supplementaire pour le jump qui permet ainsi de check si on peut walljump aussi
        if (_wallJumpCooldown > 0.3f) {
            //velocity est la vitesse du corps, on peut simuler du mouvement a droite et a gauche ici
            //on ne change que la variable x
            //velocity est un vecteur
            _body.velocity = new Vector2(_horizontalInput*speed, _body.velocity.y);

            if (isOnWall() && !isGrounded()) {
                    _body.gravityScale = 5;
                    _body.velocity = new Vector2(0, -_wallSlideSpeed);
            }
            else {
                _body.gravityScale = 5.5f;
            }
            // Cette condition permet de sauter en appuyant sur space
            if (Input.GetKey(KeyCode.Space)) {
                Jump();
            }
        }
        else {
            _wallJumpCooldown += Time.deltaTime;
        }

    }
    //methode pour jump, contient de la logique pour le wall jump aussi
    private void Jump()
    {
        if (isGrounded())
        {
            _body.velocity = new Vector2(_body.velocity.x, jumpPower);
            _anim.SetTrigger("jump");
        }
        else if (isOnWall() && !isGrounded())
        {
            RaycastHit2D wallHit = Physics2D.BoxCast(_boxCollider.bounds.center, _boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);

            if (wallHit.collider != null)
            {
                Vector2 wallNormal = wallHit.normal;

                if (_horizontalInput == 0)
                {
                    // Apply a strong force away from the wall along its normal
                    _body.velocity = wallNormal * (horizontalWalljumpPower * 3);
                    transform.localScale = new Vector3(-Mathf.Sign(wallNormal.x), transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    // Apply a combined force of horizontal and vertical wall jump powers
                    Vector2 jumpDirection = new Vector2(wallNormal.x * horizontalWalljumpPower, verticalWalljumpPower);
                    _body.velocity = jumpDirection;
                }
            }

            _wallJumpCooldown = 0;
            _anim.SetTrigger("jump");
        }
    }


    
    
    //methode pour detecter les collisions et pour mettre a jour _grounded si besoin (_grounded n'est plus utilise)
    private void OnCollisionEnter2D(Collision2D other) {

    }
    
    //remplacement pour _grounded
    private bool isGrounded()
    {
        //beacoup d'arguments, a relire attentivement et a demander des explications si necessaire, on a d'abord l'origine de la boxcast ou le centre, puis sa taille, son angle, la direction ou l'on veut check, la distance du vecteur pour check et le layer avec lequel on veut check la collision
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(_boxCollider.bounds.center,_boxCollider.bounds.size,0,Vector2.down,0.1f, groundLayer);
        return raycastHit2D.collider != null;
    }
    
    private bool isOnWall()
    {
        //
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(_boxCollider.bounds.center,_boxCollider.bounds.size,0,new Vector2(transform.localScale.x,0),0.1f, wallLayer);
        return raycastHit2D.collider != null;
    }
    

    
}
