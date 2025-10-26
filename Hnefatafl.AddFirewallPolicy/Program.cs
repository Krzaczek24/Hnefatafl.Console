using NetFwTypeLib;

AddFirewallRule();

static void AddFirewallRule()
{
    var policy = GetFirewallPolicy();

    foreach (INetFwRule rule in policy.Rules)
        if (rule.Name == "Hnefatafl")
            return;

    policy.Rules.Add(BuildInboundRule());
    policy.Rules.Add(BuildOutboundRule());
}



static INetFwPolicy2 GetFirewallPolicy()
{
    Type policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2")!;
    return (INetFwPolicy2)Activator.CreateInstance(policyType)!;
}

static INetFwRule BuildInboundRule() => MakeFirewallRule(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);

static INetFwRule BuildOutboundRule() => MakeFirewallRule(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);

static INetFwRule MakeFirewallRule(NET_FW_RULE_DIRECTION_ direction)
{
    Type ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule")!;
    INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(ruleType)!;
    firewallRule.Enabled = true;
    firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
    firewallRule.Name = "Hnefatafl";
    firewallRule.Description = "Allow connection for Hnefatafl game.";
    firewallRule.InterfaceTypes = "All";
    firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
    firewallRule.LocalPorts = "7777";
    firewallRule.RemotePorts = "7777";
    firewallRule.Profiles = (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
    firewallRule.Direction = direction;
    return firewallRule;
}