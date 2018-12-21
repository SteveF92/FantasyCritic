import fontawesome from '@fortawesome/fontawesome'
// Official documentation available at: https://github.com/FortAwesome/vue-fontawesome
import { FontAwesomeIcon, FontAwesomeLayers, FontAwesomeLayersText } from '@fortawesome/vue-fontawesome'


// If not present, it won't be visible within the application. Considering that you
// don't want all the icons for no reason. This is a good way to avoid importing too many
// unnecessary things.
fontawesome.library.add(
  require('@fortawesome/fontawesome-free-solid/faEnvelope'),
  require('@fortawesome/fontawesome-free-solid/faGraduationCap'),
  require('@fortawesome/fontawesome-free-solid/faHome'),
  require('@fortawesome/fontawesome-free-solid/faList'),
  require('@fortawesome/fontawesome-free-solid/faSpinner'),
  require('@fortawesome/fontawesome-free-solid/faBell'),
  require('@fortawesome/fontawesome-free-solid/faExternalLinkAlt'),
  require('@fortawesome/fontawesome-free-solid/faBars'),
  require('@fortawesome/fontawesome-free-solid/faTrophy'),
  require('@fortawesome/fontawesome-free-solid/faUserFriends'),
  require('@fortawesome/fontawesome-free-solid/faListOl'),
  require('@fortawesome/fontawesome-free-solid/faDollarSign'),
  require('@fortawesome/fontawesome-free-solid/faCircle'),
  require('@fortawesome/fontawesome-free-solid/faCheck'),
  //Regular
  require('@fortawesome/fontawesome-free-regular/faCircle'),
  // Brands
  require('@fortawesome/fontawesome-free-brands/faFontAwesome'),
  require('@fortawesome/fontawesome-free-brands/faMicrosoft'),
  require('@fortawesome/fontawesome-free-brands/faVuejs'),
  require('@fortawesome/fontawesome-free-brands/faTwitterSquare'),
  require('@fortawesome/fontawesome-free-brands/faRedditSquare')
)

export {
  FontAwesomeIcon,
  FontAwesomeLayers,
  FontAwesomeLayersText
}
