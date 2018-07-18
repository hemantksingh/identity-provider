function CreateDb($server, $name) {
  if ($server.Databases.Contains($name)) {
    Write-Host "Db '$name' already exists."
    return
  }
  Write-Host "Creating db '$name'"
  $db = New-Object `
    -TypeName Microsoft.SqlServer.Management.Smo.Database `
    -argumentlist $server, $name
  $db.Create()

  return $db
}

function Get-Db(
  [Parameter(mandatory=$true)]
  [Microsoft.SqlServer.Management.Smo.Server] $server,
  [Parameter(mandatory=$true)]
  [string] $name) {
  if ($server.Databases.Contains($name)) {
    return $server.Databases.Item($name)
  } else {
    Write-Host "No database '$name' found on server '$server'"
  }
}

function CreateLogin($server, $loginName, $password) {
  if ($server.Logins.Contains($loginName)) {
    Write-Host "Login '$loginName' already exists."
    return
  }

  $login = New-Object `
    -TypeName Microsoft.SqlServer.Management.Smo.Login `
    -ArgumentList $server, $loginName

  if($password) {
     Write-Host "Creating SqlLogin '$loginName' on '$server'"
    $login.LoginType = [Microsoft.SqlServer.Management.Smo.LoginType]::SqlLogin
    $login.PasswordExpirationEnabled = $false
    $login.Create($password)
  } else {
    Write-Host "Creating WindowsUser '$loginName' on '$server'"
    $login.LoginType = [Microsoft.SqlServer.Management.Smo.LoginType]::WindowsUser
    $login.Create()
  }
  return $login
}

function AddUserToDb($db, $username) {
  if($db.Users.Contains($username)) {
    Write-Host "'$username' already exists."
    return
  }

  Write-Host "Adding user '$username' to db '$db'"
  $usr = New-Object `
    -TypeName Microsoft.SqlServer.Management.Smo.User `
    -argumentlist $db, $username
  $usr.Login = $username
  $usr.Create()
}

function AddLoginToServerRole($server, $loginName, $roleName) {
  Write-Host "Adding login '$loginName' to role '$roleName'"
  $role = $server.Roles[$roleName]
  $role.AddMember($loginName)
  $role.Alter
}

function AddUserToRole($db, $user, $roleName) {
  Write-Host "Adding user '$user' to role '$roleName'"
  $role = $db.Roles[$roleName]
  $role.AddMember($user)
  $role.Alter
}